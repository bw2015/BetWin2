
var video = $(document.getElementById("video"));

var gameinfo = {
    "ID": location.href.get("id"),
    "Bet": ["石头", "剪刀", "布"],
    "Result": {
        "石头": "rock",
        "剪刀": "scissors",
        "布": "paper"
    },
    "Times": 0
}
// 设置高度
!function () {
    var height = $(document.body).getHeight().toInt();
    video.setStyle("height", height - 100);

    var list = $$("footer ul a");
    var current = null;
    list.addEvent("click", function () {
        if (current) current.removeClass("current");
        var obj = this;
        obj.addClass("current");
        current = obj;
        gameinfo.Times = this.get("data-times");
    });
    list[0].fireEvent("click");
}();

// 视频互动
!function () {

    var playlist = new Array();
    playlist.push("start");
    var playindex = 0;

    var play = function (type) {
        video.src = "images/game_" + type + ".mp4";
        if (type == "readly") {
            video.loop = true;
        } else {
            video.loop = false;
        }
        video.play();
    };

    video.addEvent("contextmenu", function () { return false; });
    document.getElementById("video").addEventListener("ended", function (e) {
        var src = e.target.src;
        type = /game_(.*).mp4/.exec(src)[1];
        play.delay(500, this, ["readly"]);
    });

    play("start");

    var action = $$("footer .action a[data-action]");
    action.addEvent("click", function () {
        //[{"id":"28","number":"8,9","mode":"元","times":1,"reward":652.68,"bet":2,"money":4}]:
        var body = $(document.body);
        var obj = this;
        var action = obj.get("data-action");
        var data = [{
            "id": gameinfo.ID,
            "number": action,
            "mode": "一元",
            "times": gameinfo.Times
        }];

        var headers = new Object();
        if (location.href.get("session")) {
            headers["USER"] = location.href.get("session");
        }
        new Request.JSON({
            "url": "/handler/game/slot/save",
            "headers": headers,
            "onRequest": function () {
                body.addClass("loading");
            },
            "onComplete": function () {
                body.removeClass("loading");
            },
            "onSuccess": function (result) {
                if (!result.success) {
                    new BW.Tip(result.msg);
                    return;
                }
                var match = null;
                var resultNumber = gameinfo.Result[result.info.ResultNumber];
                switch (gameinfo.Bet.indexOf(result.info.ResultNumber) - gameinfo.Bet.indexOf(action)) {
                    case -1:
                    case 2:
                        match = "win";
                        break;
                    case 0:
                        match = "tie";
                        break;
                    case -2:
                    case 1:
                        match = "lose";
                        break;
                }
                if (!resultNumber || !match) {
                    alert("结果错误");
                    return;
                }
                play(resultNumber + "_" + match);
            }
        }).post(JSON.encode(data));

    });
}();