// Overall viewmodel for this screen, along with initial state
function ViewModel(hub) {
    var self = this;

    self.hub = hub;

    self.guess = ko.observable("");
    self.currentTime = ko.observable(new Date());
    self.gameEndTime = ko.observable(new Date());
    self.gameStartTime = ko.observable(new Date());
    self.isGameRunning = ko.observable(false);
    self.hint = ko.observable("");
    self.message = ko.observable("");

    self.gameTimeLeft = ko.computed(function () {
        var seconds = (self.gameEndTime().getTime() - self.currentTime().getTime()) / 1000;
        return Math.ceil(seconds);
    });

    self.gameWaitLeft = ko.computed(function () {
        var seconds = (self.gameStartTime().getTime() - self.currentTime().getTime()) / 1000;
        return Math.ceil(seconds);
    });

    self.intervalId = setInterval(function () {
        self.currentTime(new Date());
    }, 100);

    
    self.hub.client.gameStart = function (hintLetter, approxEnd) {
        self.isGameRunning(true);
        self.gameEndTime(new Date(approxEnd));
        self.hint(hintLetter);

        $("#messages").html("");
    };

    self.hub.client.gameEnd = function (message, approxStart) {
        self.isGameRunning(false);
        self.gameStartTime(new Date(approxStart));
        self.message(message);
        $("#messages").html("");
        var messages = message.split("\n");
        for (var i in messages) {
            $("#messages").prepend("<li>" + messages[i] + "</li>");

        }
    };

    self.hub.client.gameStatus = function (isRunning, hint, approxNextState) {
        self.isGameRunning(isRunning);
        if (isRunning) {
            self.gameEndTime(new Date(approxNextState));
            self.hint(hint);
        }
        else {
            self.gameStartTime(new Date(approxNextState));
        }
    };

    self.submitGuess = function () {
        self.hub.server.setGuess(self.guess())
            .done(function () {
                $("#messages").prepend("<li>Guess submitted</li>");
            });
    }
};

$(function () {
    $.connection.hub.start()
        .done(function () {
            gameHub.server.registerPlayer();
        })
        .fail(function () { alert("Could not Connect!"); });

    var gameHub = $.connection.scattergoramentHub;

    ko.applyBindings(new ViewModel(gameHub));

});