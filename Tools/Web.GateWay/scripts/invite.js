var strVar = "";
strVar += "<div id=\"js-alert-box\" class=\"alert-box\" style=\"display: block;\">";
strVar += "        <svg class=\"alert-circle\" width=\"234\" height=\"234\">";
strVar += "            <circle cx=\"117\" cy=\"117\" r=\"108\" fill=\"#FFF\" stroke=\"#43AEFA\" stroke-width=\"17\"><\/circle>";
strVar += "            <circle id=\"js-sec-circle\" class=\"alert-sec-circle\" cx=\"117\" cy=\"117\" r=\"108\" fill=\"transparent\" stroke=\"#F4F1F1\" stroke-width=\"18\" transform=\"rotate(-90 117 117)\"><\/circle>";
strVar += "            <text class=\"alert-sec-unit\" x=\"82\" y=\"172\" fill=\"#BDBDBD\">secs<\/text>";
strVar += "        <\/svg>";
strVar += "        <div id=\"js-sec-text\" class=\"alert-sec-text\">0<\/div>";
strVar += "        <div class=\"alert-body\">";
strVar += "            <div id=\"js-alert-head\" class=\"alert-head\">前往会员注册<\/div>";
strVar += "            <div class=\"alert-concent\">";
strVar += "                <p>花个3分钟创建属于您的个人帐号<\/p>";
strVar += "                <p>您将在我们的网站享受更多<\/p>";
strVar += "            <\/div>";
strVar += "            <a id=\"js-alert-btn\" class=\"alert-btn\" href=\"javascript:location.reload();\">立即前往注册<\/a>";
strVar += "        <\/div>";
strVar += "        <div class=\"alert-footer clearfix\">";
strVar += "            <svg width=\"46px\" height=\"42px\" class=\"alert-footer-icon\">";
strVar += "                <circle fill-rule=\"evenodd\" clip-rule=\"evenodd\" fill=\"#7B7B7B\" stroke=\"#DEDFE0\" stroke-width=\"2\" stroke-miterlimit=\"10\" cx=\"21.917\" cy=\"21.25\" r=\"17\"><\/circle>";
strVar += "                <path fill=\"#FFF\" d=\"M22.907,27.83h-1.98l0.3-2.92c-0.37-0.22-0.61-0.63-0.61-1.1c0-0.71,0.58-1.29,1.3-1.29s1.3,0.58,1.3,1.29 c0,0.47-0.24,0.88-0.61,1.1L22.907,27.83z M18.327,17.51c0-1.98,1.61-3.59,3.59-3.59s3.59,1.61,3.59,3.59v2.59h-7.18V17.51z M27.687,20.1v-2.59c0-3.18-2.59-5.76-5.77-5.76s-5.76,2.58-5.76,5.76v2.59h-1.24v10.65h14V20.1H27.687z\"><\/path>";
strVar += "                <circle fill-rule=\"evenodd\" clip-rule=\"evenodd\" fill=\"#FEFEFE\" cx=\"35.417\" cy=\"10.75\" r=\"6.5\"><\/circle>";
strVar += "                <polygon fill=\"#7B7B7B\" stroke=\"#7B7B7B\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-miterlimit=\"10\" points=\"35.417,12.16 32.797,9.03 31.917,10.07 35.417,14.25 42.917,5.29 42.037,4.25 \"><\/polygon>";
strVar += "            <\/svg>";
strVar += "            <div class=\"alert-footer-text\">";
strVar += "                <p>secure<\/p>安全加密";
strVar += "            <\/div>";
strVar += "        <\/div>";
strVar += "    <\/div>";



!function () {

    var domain = DOMAIN;
    if (domain.length == 0) {
        alert("链接错误");
        location.href = "index.html";
        return;
    }

    document.writeln(strVar);

    var testSpeed = new Array();
    var testError = new Array();
    for (var i = 0; i < domain.length; i++) {
        !function () {
            var item = domain[i];
            var path = "//" + item + "/images/logo.png?rnd=" + Math.random();
            var now = new Date().getTime();

            var img = document.createElement("img");
            img.src = path;
            img.onload = function () {
                var time = new Date().getTime() - now;
                var result = {
                    "time": time,
                    "domain": "http://" + item + "/register.html#" + ID
                };
                testSpeed.push(result);
            };
            img.onerror = function () {
                testError.push({
                    "domain": item
                });
            };
        }();
    };

    function alertSet(e) {
        document.getElementById("js-alert-box").style.display = "block", document.getElementById("js-alert-head").innerHTML = e;
        var t = 5, n = document.getElementById("js-sec-circle");
        document.getElementById("js-sec-text").innerHTML = t,
        si = setInterval(function () {
            if (0 == t) {
                clearInterval(si);
                if (testSpeed.length == 0) {
                    var wait = setInterval(function () {
                        if (testError.length == domain.length) {
                            clearInterval(wait);
                            location.href = testSpeed[0].domain;
                        } else if (testSpeed.length > 0) {
                            clearInterval(wait);
                            location.href = testSpeed[0].domain;
                        }
                    }, 100);
                   
                } else {
                    location.href = testSpeed[0].domain;
                }
            } else {
                t -= 1, document.getElementById("js-sec-text").innerHTML = t;
                var e = Math.round(t / 5 * 735);
                n.style.strokeDashoffset = e - 735
            }
        }, 970);
    };

    alertSet('前往会员注册');
}();


