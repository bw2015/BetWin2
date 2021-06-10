//获取配置值

var KEY = "BETWIN 2.0";

function GetConfig() {
    var config = new Object();
    var betwin = localStorage.getItem(KEY);
    if (betwin) config = JSON.decode(betwin);
    return config;
}

function SetConfig(config) {
    localStorage.setItem(KEY, JSON.encode(config));
}

// 获取本地存储的信息
chrome.runtime.onMessage.addListener(function (data, sender, callback) {
    var Config = GetConfig();
    callback(Config);
});