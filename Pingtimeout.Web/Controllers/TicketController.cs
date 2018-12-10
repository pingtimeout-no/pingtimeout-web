﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PingTimeout.Web.Data;
using PingTimeout.Web.Helpers;
using PingTimeout.Web.Models;

namespace PingTimeout.Web.Controllers
{
    public class TicketController : Controller
    {

        private readonly IOptions<PingTimeoutConfig> config;
        private ApplicationDbContext context;
 
        public TicketController(ApplicationDbContext context, IOptions<PingTimeoutConfig> config)
        {
            this.context = context;
            this.config = config;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var model = new SeatmapIndexViewModel();
            var activeEvent = context.Events.FirstOrDefault();

            if (activeEvent == null)
                return NotFound();

            var tickets = context.Tickets.Where(t => t.Event.Id == activeEvent.Id).OrderBy(t => t.PurchaseDate); 

            ViewData["BaseAuthUrl"] = config.Value.BaseUrl + "seatmap/auth?token=";
            
            return View(tickets);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Import()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SendTokens() {

            int i = 0;

            var activeEvent = context.Events.FirstOrDefault();

            if (activeEvent == null)
                return NotFound();

            var tickets = context.Tickets.Where(t => t.Event.Id == activeEvent.Id);
            foreach (var ticket in tickets) {
                i++;
            }

            return Ok("Sendt til " + i + " stykk");

        }


        [Authorize(Roles = "Admin")]
        [HttpPost("UploadFile")]
        public IActionResult UploadFile()
        {
            var activeEvent = context.Events.FirstOrDefault();

            if (activeEvent == null)
                return NotFound();

            // full path to file in temp location
            var filePath = Path.GetTempPath();
            IFormFile file = Request.Form.Files[0];

            if (file.Length > 0)
            {
                string sFileExtension = Path.GetExtension(file.FileName).ToLower();
                ISheet sheet;
                string fullPath = Path.Combine(filePath, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    if (sFileExtension == ".xls")
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                    }
                    else
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                    }

                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) break;
                        
                        try
                        {
                            var ticketNumber = int.Parse(row.GetCell(12)?.ToString());
                            var pickupCode = row.GetCell(16)?.ToString();
                            if (ticketNumber > 0 && !string.IsNullOrWhiteSpace(pickupCode))
                            {
                                if (!context.Tickets.Any(t => t.TicketNumber == ticketNumber))
                                {
                                    var purchaserName = row.GetCell(0).ToString();
                                    var purchaserEmail = row.GetCell(2).ToString();
                                    var purchaseDate = DateTime.Parse(row.GetCell(17).ToString());
                                    var userName = row.GetCell(6).ToString();
                                    var userEmail = row.GetCell(7).ToString();
                                    var userBirthdateString = row.GetCell(8).ToString();

                                    // Handle bad dates
                                    DateTime? userBirthdate;
                                    if (userBirthdateString.Length == 6) {
                                        var provider = CultureInfo.InvariantCulture;
                                        userBirthdate = new DateTime(Int16.Parse("20"+userBirthdateString.Substring(4, 2)), Int16.Parse(userBirthdateString.Substring(2, 2)), Int16.Parse(userBirthdateString.Substring(0, 2)));
                                        userBirthdate = DateTime.ParseExact(userBirthdateString, "mmDDyy", provider);
                                    } else if (string.IsNullOrWhiteSpace(userBirthdateString)) {
                                        userBirthdate = null;
                                    } else {
                                        userBirthdate = DateTime.Parse(userBirthdateString);
                                    }

                                    var token = string.Empty;
                                    while (string.IsNullOrWhiteSpace(token) ||
                                           context.Tickets.Any(t => t.SeatMapToken == token)) {
                                        token = TokenHelper.RandomString(6);
                                    }

                                    var ticket = new Ticket()
                                    {
                                        Event = activeEvent,
                                        ImportDate = DateTime.Now,
                                        TicketNumber = ticketNumber,
                                        PickupCode = pickupCode,
                                        PurchaserName = purchaserName,
                                        PurchaserEmail = purchaserEmail,
                                        PurchaseDate = purchaseDate,
                                        UserName = userName,
                                        UserEmail = userEmail,
                                        UserBirthdate =  userBirthdate,
                                        SeatMapToken = token
                                    };
                                    context.Tickets.Add(ticket);
                                }
                            }
                        } catch (Exception ex) {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }
    }
}