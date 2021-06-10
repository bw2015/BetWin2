var SUCCESS = null;
var regex = /\/(wx|mobile|mobile3|pc)\/\d{4}/;
var search = location.search;
if (regex.test(search)) {
    var type = regex.exec(search)[1];
    DOMAINLIST.each(function (item, index) {
        var url = item + "/handler/system/config/ping";
        (function () {
            if (SUCCESS) return;
            new Request.JSON({
                "url": url,
                "onComplete": function () {
                    console.log("complete" + item);
                },
                "onFailure": function () {
                    console.log(item);
                },
                "onSuccess": function (result) {
                    if (!SUCCESS) {
                        var url = item;
                        switch (type) {
                            case "wx":
                                url += "/wechat/";
                                break;
                            case "pc":
                                url += "/";
                                break;
                            default:
                                url += ("/" + type + "/");
                                break;
                        }
                        location.href = url;
                    }
                }
            }).post();
        }).delay(index * 500);
    });
}

window.onload = function () {
    !function () {
        var img = $$(".start img").map(function (item) { return { "content": item.get("src") }; });
        $("start").dispose();
        document.body.removeClass("loading");
        new iSlider(document.body, img, {
            isLooping: 1,
            isOverspread: 1,
            isAutoplay: 1,
            animateTime: 800,
            animateType: 'flow'
        });
    }();
};