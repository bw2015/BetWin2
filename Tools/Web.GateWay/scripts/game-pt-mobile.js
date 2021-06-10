// PT移动端

iapiSetCallout('Login', calloutLogin);

var INFO = {
    "playername": null,
    "password": null,
    "key": null,
    "language": "zh-cn",
    "gametype": null
};

// 开始登录
function login() {
    INFO.playername = document.getElementById("playername").value;
    INFO.password = document.getElementById("password").value;
    INFO.key = document.getElementById("key").value;

    iapiSetClientPlatform("mobile&deliveryPlatform=HTML5");
    var realMode = 1;
    iapiLogin(INFO.playername, INFO.password, realMode, "en");
}

// 登录后的回调信息
function calloutLogin(response) {
    if (response.errorCode) {
        document.body.innerHTML = "Login failed. " + response.playerMessage + " Error code: " + response.errorCode;
    }
    else {
        document.body.innerHTML = "PT帐号登录成功，正在装载游戏...";
        lobby();
    }
}

// 进入游戏的回调信息
function lobby() {
    iapiSetCallout('GetTemporaryAuthenticationToken', calloutGetTemporaryAuthenticationToken);

    setTimeout(function () {
        askTempandLaunchGame("ngm");
    }, 1000);
}

// 进入游戏
function askTempandLaunchGame(type) {
    INFO.gametype = type;
    var realMode = 1;

    iapiRequestTemporaryToken(realMode, '424', 'GamePlay');
};

function launchMobileClient(temptoken) {
    if (INFO.gametype == "mps") {
        var clientUrl = '' + '?username=' + INFO.playername + '&temptoken=' + temptoken + '&game=' + INFO.key + '&real=1';
    } else if (INFO.gametype == "ngm") {
        //http://hub.ld176888.com/igaming/?gameId=ano&real=1&username=HN8037511E6F7&lang=en&tempToken=IH3JpoOA0Lmm1pBInihBADAA8CBQYCA4&lobby=http://betniu88.com/pt/lobby.html&support=http://betniu88.com/pt/support.html&logout=http://betniu88.com/pt/logout.html

        var clientUrl = 'http://hub.ld176888.com/igaming/?gameId=' + INFO.key + '&real=1' + '&username=' + INFO.playername + '&lang=' + INFO.language +
            '&tempToken=' + temptoken +
            '&lobby=http://betniu88.com/pt/lobby.html&support=http://betniu88.com/pt/support.html&logout=http://betniu88.com/pt/logout.html';
    }
    document.location = clientUrl;
};

function calloutGetTemporaryAuthenticationToken(response) {
    if (response.errorCode) {
        alert("Token failed. " + response.playerMessage + " Error code: " + response.errorCode);
    }
    else {
        launchMobileClient(response.sessionToken.sessionToken);
    }
}



window.onload = login;

//  进入游戏的方法
//  askTempandLaunchGame('ngm', 'ano')
