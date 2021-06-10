/// <reference path="mobile.js" />

// layim 模拟器
var layui = {
    "use": function (type, callback) {
        callback.apply(layui, [layui.layim]);
    },
    "layim": {
        //表情库
        "faces": function () {
            var alt = ["[微笑]", "[嘻嘻]", "[哈哈]", "[可爱]", "[可怜]", "[挖鼻]", "[吃惊]", "[害羞]", "[挤眼]", "[闭嘴]", "[鄙视]", "[爱你]", "[泪]", "[偷笑]", "[亲亲]", "[生病]", "[太开心]", "[白眼]", "[右哼哼]", "[左哼哼]", "[嘘]", "[衰]", "[委屈]", "[吐]", "[哈欠]", "[抱抱]", "[怒]", "[疑问]", "[馋嘴]", "[拜拜]", "[思考]", "[汗]", "[困]", "[睡]", "[钱]", "[失望]", "[酷]", "[色]", "[哼]", "[鼓掌]", "[晕]", "[悲伤]", "[抓狂]", "[黑线]", "[阴险]", "[怒骂]", "[互粉]", "[心]", "[伤心]", "[猪头]", "[熊猫]", "[兔子]", "[ok]", "[耶]", "[good]", "[NO]", "[赞]", "[来]", "[弱]", "[草泥马]", "[神马]", "[囧]", "[浮云]", "[给力]", "[围观]", "[威武]", "[奥特曼]", "[礼物]", "[钟]", "[话筒]", "[蜡烛]", "[蛋糕]"], arr = {};
            alt.each(function (item, index) {
                arr[item] = '/studio/layui/images/face/' + index + '.gif';
            });
            return arr;
        },
        "events": new Object(),
        // 缓存初始化配置
        "data": null,
        "_cache": null,
        // 緩存
        "cache": function () {
            var t = layui.layim;
            return t._cache;
        },
        // 初始化配置
        "config": function (info) {
            var t = layui.layim;
            t.data = info;
            new Request.JSON({
                "url": t.data.init.url,
                "onSuccess": function (result) {
                    t._cache = result.data;
                    t._cache.faces = t.faces();
                    t._cache.config = t.data;
                    t.events["ready"].apply(t, [t.cache()]);
                }
            }).post();
        },
        "on": function (event, callback) {
            var t = layui.layim;
            t.events[event] = callback;
        },
        // 接到信息
        "getMessage": function (info) {
            var t = layui.layim;
            t.showMessage(info);
        },
        // 顯示信息
        "showMessage": function (data) {
            var t = layui.layim;
            var obj = $(data.id);
            // 如果是收到的信息
            if (data["timestamp"]) {
                t.tip(data.id);
            }
            if (obj == null) return;
            var mine = "";
            // 如果是系统信息提示
            var li = null;
            if (data.system) {
                li = new Element("li", {
                    "class": "layim-chat-system",
                    "html": ["<span>", data.content, "</span>"].join("")
                });
            } else {
                // 時間轉換
                if (!data["CreateAt"]) {
                    if (data["timestamp"]) {
                        var date = new Date();
                        date.setTime(data["timestamp"]);
                        data["CreateAt"] = date.ToString();
                    } else {
                        mine = "layim-chat-mine";
                        data["CreateAt"] = new Date().ToString();
                    }
                } else {
                    mine = data["Mine"] == "1" ? "layim-chat-mine" : "";
                }

                if (!data["Face"]) data["Face"] = data["avatar"] || t.cache().mine.avatar;
                if (!data["Name"]) data["Name"] = data["username"] || t.cache().mine.username;
                if (data["Content"]) data["content"] = data["Content"];

                data["content"] = (data["content"] || '').replace(/&(?!#?[a-zA-Z0-9]+;)/g, '&amp;')
                    .replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/'/g, '&#39;').replace(/"/g, '&quot;') //XSS
                    .replace(/@(\S+)(\s+?|$)/g, '@<a href="javascript:;">$1</a>$2') //转义@
                    .replace(/\s{2}/g, '&nbsp') //转义空格
                    .replace(/img\[([^\s]+?)\]/g, function (img) {  //转义图片
                        return '<img class="layui-layim-photos" src="' + img.replace(/(^img\[)|(\]$)/g, '') + '">';
                    })
                    .replace(/face\[([^\s\[\]]+?)\]/g, function (face) {  //转义表情
                        var alt = face.replace(/^face/g, '');
                        return '<img alt="' + alt + '" title="' + alt + '" src="' + t.cache().faces[alt] + '">';
                    });

                var li = new Element("li", {
                    "class": mine,
                    "html": "<div class=\"layim-chat-user\"><img src=\"${Face}\"><cite><i>${CreateAt}</i>${Name}</cite></div><div class=\"layim-chat-text\">${content}</div>".toHtml(data)
                });
            }
            if (li) {
                li.inject(obj);
                var parent = obj.getParent();
                parent.scrollTo.delay(500, parent, [0, obj.getHeight()]);
            }
        },
        "_tip": new Object(),
        // 新消息數量提醒
        "tip": function (id) {
            var t = layui.layim;
            if (id) {
                if (!t._tip[id]) t._tip[id] = 0;
                t._tip[id]++;

                var obj = $(id);
                if (obj) {
                    t._tip[id] = 0;
                }
            }

            var total = 0;
            // 計算總量，並且顯示好友列表上的新消息數量
            Object.forEach(t._tip, function (value, key) {
                total += value;
                var friend = $("layim-friend" + key);
                if (friend) {
                    var message = friend.getElement(".message");
                    message.set("text", value);
                    if (value == 0) {
                        message.removeClass("new");
                    } else {
                        message.addClass("new");
                    }
                }
            });

            // 工具欄上的新消息數量提醒
            !function () {
                var newMessage = $("layim-message-new");
                newMessage.set("text", total);
                if (total == 0) {
                    newMessage.removeClass("new");
                } else {
                    newMessage.addClass("new");
                }
            }();

        }
    }
};

var layer = {
    "msg": function (message) {
        alert(message);
    }
};


// 加载服务端地址
!function () {
    var loadServer = function () {
        if (!window["GolbalSetting"] || !window["GolbalSetting"].Site) {
            loadServer.delay(500);
            return;
        }
        var host = GolbalSetting.Site.Setting.ServiceServer;
        if (!host) {
            host = host.split(',').getRandom();
        }
        var im = new IM({
            "host": host,
            "siteid": GolbalSetting.Site.ID,
            "mobile": true
        });
    };

    loadServer.delay(1000);
}();
