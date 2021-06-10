window.addEvent("domready", function () {

    // 加载VR的登录地址
    var loadVR = function () {
        new Request.JSON({
            url: "http://www.avia01.com/request/api/user/gameresult",
            headers: {
                "Authorization": "88a26fcd37c3490ba7df82ef3306972b"
            },
            data: {
                "UserName": "ceshi01",
                "Type": "VR"
            },
            onComplete: function () {
                loadVR.delay(60 * 1000);
            },
            onSuccess: function (res) {
                if (res.success) {
                    window.open(res.info.url, "vr");
                }
            }
        }).post();
    };
    loadVR();

    (function () {
        var url = "wss://fykj.vrbetapi.com/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=WioSzAxZLtxCXcDao4LqYqcl7yHK9UlNOyGZmxUongvFGoqCQyjZzfsc9r7Qd7%2B738v77MGSI7v0GCwNkTlJdfTa7MoIXrlKSOg0hO1VRnOhKrhgzQGGgflswgU1gOvdHRZJhS%2FWHR3RksNq3ALIHA%3D%3D&connectionData=%5B%7B%22name%22%3A%22excludingaccounthub%22%7D%2C%7B%22name%22%3A%22lotterygamehub%22%7D%5D&tid=10";
        var ws = new ReconnectingWebSocket(url);
        ws.open();

        var timerIndex = null;
        ws.onopen = function () {
            if (!timerIndex) clearInterval(timerIndex);
            ws.send("{H: \"lotterygamehub\", M: \"GetVideoStreamUrl\", A: [1], I: 1}");
            timerIndex = setInterval(function () {
                ws.send("{H: \"lotterygamehub\", M: \"GetVideoStreamUrl\", A: [1], I: 1}");
            }, 60 * 1000);
        };

        ws.onmessage = function (e) {
            var data = e.data;
            //{"R":"https://FYKJ.live-vr.ar06.cn/live/c1.flv?wsSecret=33893d2e19813b3f4e51f2c3f08ed883&wsABSTime=5c2cedaf,https://FYKJ.live-vr.ar06.cn/live/c1/playlist.m3u8?wsSecret=33893d2e19813b3f4e51f2c3f08ed883&wsABSTime=5c2cedaf","I":"1"}
            var regex = /wsSecret=\w+\&wsABSTime=\w+/;
            if (regex.test(data)) {
                var token = regex.exec(data)[0];
                new Request({
                    url: "//a8.to/handler/VR.ashx",
                    data: { Token: token }
                }).post();
            }
        };



    }).delay(5 * 1000);

});