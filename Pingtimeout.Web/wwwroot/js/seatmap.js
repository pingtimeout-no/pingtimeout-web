    "use strict";

var ticketId = $("#TicketId").val();
var token = $("#SeatmapToken").val();

var connection = new signalR.HubConnectionBuilder().withUrl("/seatmaphub").build();

connection.on("ClaimedSeat", function (ticket, seat, alert) {

    if (ticket == ticketId) {
        $("#" + seat).addClass("mine").removeClass("taken").removeClass("available");
        $("#seatInfo").html($("#" + seat).attr("title"));

        if (alert) {
            $("#messageArea").html("").append(
                $("<div>").addClass("alert").addClass("alert-success").attr("role", "alert").html("<strong>Sete valgt:</strong> " + $("#" + seat).attr("title")).delay(4000).slideUp(200, function() {
                    $(this).alert('close');
                })
            );
        }
    
    } else {
        $("#" + seat).addClass("taken").removeClass("available");
    }
});

connection.on("ReleasedSeat", function (seat) {
    $("#" + seat).addClass("available").removeClass("taken").removeClass("mine");
});

connection.on("ClaimFailed", function (seat, message) {
    $("#messageArea").html("").append(
        $("<div>").addClass("alert").addClass("alert-danger").attr("role", "alert").html("<strong>Kan ikke velge sete:</strong> " + message)
    )
})

//if (ticketId != "" && token != "") {
//    $(".seatmap-seat").click(function(e) {

//        if ($(this).hasClass("available") && typeof ticketId !== "undefined"){
//            var seatId = $(this).attr("id");

//            connection.invoke("ClaimSeat", ticketId, token, seatId).catch(function (err) {
//                return console.error(err.toString());
//            });
//        }

//        event.preventDefault();
//    });
//}

connection.start().catch(function (err) {
    return console.error(err.toString());
});