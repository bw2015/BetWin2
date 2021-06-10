window.addEvent("domready", function () {
    var url = location.href;
    if (!url.contains("#")) return;
    var type = url.substr(url.lastIndexOf("#") + 1);

    var iframe = $("iframe");
    switch (type) {
        case "ChungKing":
            new Element("iframe", {
                "src": "http://360.chart.betwin.ph/zst/ssccq",
                "styles": {
                    "border": "none",
                    "width": "100%",
                    "height": UI.getSize().y,
                    "margin": "auto"
                }
            }).inject(iframe);
            break;
        default:
            alert("正在开发中");
            window.close();
            break;
    }
});