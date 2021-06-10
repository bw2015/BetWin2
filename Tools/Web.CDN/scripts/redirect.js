!function () {
    var site = window["SITE"];
    console.log(site);
    if (!site) return;
    document.title = site.Name;
    var domainlist = site.Domain;
    if (domainlist.length == 0) {
        alert("未设置域名");
        return;
    }
    var domain = domainlist[0];
    document.getElementById("entry").href = "//" + domain + location.pathname + location.search;
}()