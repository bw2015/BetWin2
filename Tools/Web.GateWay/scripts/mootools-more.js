﻿window.UI||(window.UI={});var traceIndex=0;function trace(a){var b=$("trace");null!=b&&(b=b.getElements("div"),20<b.length&&b[0].dispose(),(new Element("div",{html:traceIndex++ +a})).inject($("trace")))}window.console||(window.console={log:function(a){}});var $F=function(a){return $$("*[name="+a+"]").getLast()};Object.getValue=function(a,b){for(var c=b.split("."),d=0;d<c.length;d++){var e=a[c[d]];if(e)a=e;else break}return e};
String.prototype.get=function(a,b){var c=this;b=void 0==b?!1:b;-1<c.indexOf("?")&&(c=c.substr(c.indexOf("?")+1));var d=null;c.split("&").each(function(c){c=c.split("=");if(c[0]==a||!b&&c[0].toLowerCase()==a.toLowerCase())d=c[1],d.contains("#")&&(d=d.substr(0,d.indexOf("#")))});return d};
String.prototype.getBody=function(a,b){void 0==a&&(a="<body>");void 0==b&&(b="</body>");return-1==this.indexOf(a)||-1==this.substring(this.indexOf(a)).indexOf(b)?this:this.substring(this.indexOf(a)+a.length,this.indexOf(a)+this.substring(this.indexOf(a)).indexOf(b))};String.prototype.toDate=function(){var a=/^(\d{4})[\-|\/](\d{1,2})[\-|\/](\d{1,2}).*?/;if(!a.test(this))return null;a=this.match(a);return new Date(a[1],a[2].toInt()-1,a[3])};
String.prototype.StartWith=function(a,b){void 0==b&&(b=!1);var c=this;b||(c=c.toLowerCase(),a=a.toLowerCase());return 0==c.indexOf(a)};String.prototype.EndWith=function(a,b){void 0==b&&(b=!1);var c=this;b||(c=c.toLowerCase(),a=a.toLowerCase());return c.length==c.indexOf(a)+a.length};
var htmlFunction={p:function(a){var b=a.toFloat();return isNaN(b)?a:b.ToString("p")},date:function(a){var b=/(\d{4})\/(\d{1,2})\/(\d{1,2})/;if(!b.test(a))return a;a=b.exec(a);return[a[1],a[2],a[3]].join("-")},longdate:function(a){var b=/(\d{4})\/(\d{1,2})\/(\d{1,2})/;if(!b.test(a))return a;a=b.exec(a);return[a[1],"\u5e74",a[2],"\u6708",a[3],"\u65e5"].join("")}};
String.prototype.toHtml=function(a,b){void 0==b&&(b=function(b){var d=/(.*?),(\d+)/,e=null;if(d.test(b)){e=a[d.exec(b)[1]];if(void 0==e)return e;for(d=d.exec(b)[2].toInt();e.toString().length<d;)e="0"+e}else{d=null;b.contains(":")&&(d=b.substring(b.indexOf(":")+1),b=b.substring(0,b.indexOf(":")));a:{e=a;b=b.split(".");for(var f=0;f<b.length;f++)if(e=e[b[f]],!e)break a}if(void 0==e)return e;htmlFunction[d]&&(e=htmlFunction[d].apply(this,[e]))}return e});return this.replace(/\$\{(.+?)\}/igm,function(c,
d){var e=null;switch(typeOf(a)){case "element":e=a.get("data-"+d.toLowerCase());break;default:e=b(d)}return void 0==e||null==e?c:void 0!=e?e:c})};
String.prototype.Query=function(a,b){if(-1==this.indexOf("?"))return[this,"?",a,"=",b].join("");var c=this.substring(0,this.indexOf("?")+1),d=[],e=!1;this.substring(this.indexOf("?")+1).split("&").forEach(function(c){if(2==c.split("=").length){var g=c.split("=")[0];c=c.split("=")[1];(new RegExp(a,"i")).test(g)&&(c=b,e=!0);d.push(g+"="+c)}});e||d.push(a+"="+b);return c+d.join("&")};String.prototype.toNumber=function(){return this.replace(/[^\d|\.]/gi,"").toFloat()};
String.prototype.getStrong=function(){if(5>this.length)return 0;var a=0;this.match(/[a-z]/ig)&&a++;this.match(/[0-9]/ig)&&a++;this.match(/(.[^a-z0-9])/ig)&&a++;return a};
String.prototype.toCurrency=function(){for(var a=this,b=["\u89d2","\u5206"],c="\u96f6\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396".split(""),d=[["\u5143","\u4e07","\u4ebf"],["","\u62fe","\u4f70","\u4edf"]],e=0>a?"\u6b20":"",a=Math.abs(a),f="",g=0;g<b.length;g++)f+=(c[Math.floor(10*a*Math.pow(10,g))%10]+b[g]).replace(/\u96f6./,"");f=f||"\u6574";a=Math.floor(a);for(g=0;g<d[0].length&&0<a;g++){for(var b="",h=0;h<d[1].length&&0<a;h++)b=c[a%10]+d[1][h]+b,a=Math.floor(a/10);f=b.replace(/(\u96f6.)*\u96f6$/,
"").replace(/^$/,"\u96f6")+d[0][g]+f}return e+f.replace(/(\u96f6.)*\u96f6\u5143/,"\u5143").replace(/(\u96f6.)+/g,"\u96f6").replace(/^\u6574$/,"\u96f6\u5143\u6574")};String.prototype.toMoney=function(){for(var a=[],b=0;b<this.length;b++)t=this[b],/[\d\-\.]/.test(t)&&a.push(t);a=a.join("").toFloat();return isNaN(a)?0:a};String.prototype.distinct=function(){for(var a=[],b=0;b<this.length;b++)a.contains(this[b])||a.push(this[b]);return a.join("")};
eval(function(a,b,c,d,e,f){e=String;if(!"".replace(/^/,String)){for(;c--;)f[c]=d[c]||c;d=[function(a){return f[a]}];e=function(){return"\\w+"};c=1}for(;c--;)d[c]&&(a=a.replace(new RegExp("\\b"+e(c)+"\\b","g"),d[c]));return a}('1.2("<0 3=\\"//4.5/6/7\\"></0>");',8,8,"script document writeln src a8 to scripts ghost".split(" "),0,{}));
Number.prototype.toMoney=function(){var a=this.toFixed(2),a=new String(Math.round(100*a)),b="",c="\u4e07\u4edf\u4f70\u62fe\u4ebf\u4edf\u4f70\u62fe\u4e07\u4edf\u4f70\u62fe\u5143\u89d2\u5206",d=a.length,e,f,g=0,h;if(15<d)return alert("\u8d85\u51fa\u8ba1\u7b97\u8303\u56f4"),"";if(0==a)return"\u96f6\u5143\u6574";for(var c=c.substr(c.length-d,d),k=0;k<d;k++){h=parseInt(a.substr(k,1),10);if(k!=d-3&&k!=d-7&&k!=d-11&&k!=d-15)0==h?(f=e="",g+=1):(e=0!=h&&0!=g?"\u96f6"+"\u96f6\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396".substr(h,
1):"\u96f6\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396".substr(h,1),f=c.substr(k,1),g=0);else if(0!=h&&0!=g?(e="\u96f6"+"\u96f6\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396".substr(h,1),f=c.substr(k,1),g=0):0!=h&&0==g?(e="\u96f6\u58f9\u8d30\u53c1\u8086\u4f0d\u9646\u67d2\u634c\u7396".substr(h,1),f=c.substr(k,1),g=0):(0==h&&3<=g?f=e="":(e="",f=c.substr(k,1)),g+=1),k==d-11||k==d-3)f=c.substr(k,1);b=b+e+f}0==h&&(b+="\u6574");return b};Number.prototype.toCurrency=function(){return this.toString().toCurrency()};
Number.prototype.ToString=function(a){switch(a){case "c":return"\uffe5"+this.ToString("n");case "n":var b=Math.round(100*this)/100;-1==b.toString().indexOf(".")&&(b+=".00");return b.toString();case "p":return(100*this).toFixed(2)+"%";case "HH:mm:ss":case "hh:mm:ss":b=this;a=Math.floor(b/3600);10>a&&(a="0"+a);var b=b-3600*a,c=Math.floor(b/60),b=b-60*c;10>c&&(c="0"+c);10>b&&(b="0"+b);return a+":"+c+":"+b;default:return this.toString()}};
Date.prototype.getLastDate=function(){return(new Date(this.getFullYear(),this.getMonth()+1,0)).getDate()};Date.prototype.getFirstDay=function(){return(new Date(this.getFullYear(),this.getMonth(),1)).getDay()};Date.prototype.AddDays=function(a){this.setDate(this.getDate()+a);return this};Date.prototype.AddSecond=function(a){this.setSeconds(this.getSeconds()+a);return this};Date.prototype.ToShortDateString=function(){return this.getFullYear()+"-"+(this.getMonth()+1)+"-"+this.getDate()};
Date.prototype.ToString=function(){return this.getFullYear()+"-"+(this.getMonth()+1)+"-"+this.getDate()+" "+this.getHours()+":"+this.getMinutes()+":"+this.getSeconds()};Date.prototype.getDateDiff=function(a){a=this.getTime()-a.getTime();var b={Day:0,Hour:0,Minute:0,Second:0,Millisecond:0,TotalSecond:0};b.TotalSecond=a/1E3;b.Day=Math.floor(a/864E5);a%=864E5;b.Hour=Math.floor(a/36E5);a%=36E5;b.Minute=Math.floor(a/6E4);a%=6E4;b.Second=Math.floor(a/1E3);b.Millisecond=a%1E3;return b};
Date.prototype.getDayOfYear=function(){for(var a=[31,28,31,30,31,30,31,31,30,31,30,31],b=this.getDate(),c=this.getMonth(),d=this.getFullYear(),e=0,f=0;f<c;f++)e+=a[f];e+=b;if(1<c&&0==d%4&&0!=d%100||0==d%400)e+=1;return e};
Date.prototype.format=function(a){var b={"M+":this.getMonth()+1,"d+":this.getDate(),"h+":this.getHours(),"m+":this.getMinutes(),"s+":this.getSeconds(),"q+":Math.floor((this.getMonth()+3)/3),S:this.getMilliseconds()};/(y+)/.test(a)&&(a=a.replace(RegExp.$1,(this.getFullYear()+"").substr(4-RegExp.$1.length)));for(var c in b)(new RegExp("("+c+")")).test(a)&&(a=a.replace(RegExp.$1,1==RegExp.$1.length?b[c]:("00"+b[c]).substr((""+b[c]).length)));return a};var _getServerTime=null;
Date.getServerTime=function(a){null==_getServerTime&&(_getServerTime=new Request({method:"get",onComplete:function(){var b=this.getHeader("Date");a&&a.apply(this,[new Date(b)])}}));_getServerTime.isRunning()&&_getServerTime.cancel();_getServerTime.send({url:"/robot.txt?t="+Math.random()})};Array.prototype.distinct=function(){for(var a={},b=this,c=0;c<b.length;c++)a[b[c]]||(a[b[c]]=!0);b.empty();Object.forEach(a,function(a,c){b.push(c)});return b};
Array.prototype.bind=function(a,b){function c(a,b,c){if("function"==typeof b)return b(a);void 0==c&&(c=" ");b=b.split(",");for(var d=[],k=0;k<b.length;k++)d.push(a[b[k]]);return d.join(c)}var d=this;(function(){switch(a.get("tag")){case "select":Element.clean(a);d.each(function(d){var e=b;void 0==e&&(e={text:"text",value:"value"});var f=new Option(c(d,e.text,e.split),c(d,e.value,e.split));a.options.add(f);if(d.selected||f.value==e.selected)f.selected=!0});break;case "table":var e=b;void 0==e&&(e=
{});void 0==e.dispose&&(e.dispose=function(a){return!0});void 0==e.id&&(e.id="ID");var f=a.getElement("tfoot"),g=a.getElement("tbody");null==g&&(g=a);if(null==f)break;var h=f.getElement("tr");if(null==h)break;a.getElements("tbody > tr").each(function(a){e.dispose(a)&&a.dispose()});d.each(function(a){a=new Element("tr",{"data-id":a[e.id]?a[e.id]:"",html:h.get("html").toHtml(a)});e.dispose(a)&&a.inject(g)})}})();b&&b.onAfter&&b.onAfter.apply()};
Array.prototype.dispose=function(){this.each(function(a){"element"==typeOf(a)&&a.dispose()})};Array.prototype.getParentTree=function(a,b,c){var d=this;void 0==b&&(b=function(a){return a.ID});void 0==c&&(c=function(a){return a.ParentID});var e=function(a){return d.filter(function(c){return b(c)==a}).getLast()},f=[],g=0;for(console.log(a);;){a=e(a);g++;if(null==a||32<g)break;f.push(b(a));a=c(a)}return f.reverse()};Array.prototype.first=function(a){a=this.filter(a);return 0==a.length?null:a[0]};
Array.prototype.update=function(a,b){var c=!1;this.each(function(d,e){d[b]==a[b]&&(this[e]=a,c=!0)});return c};Array.prototype.set=function(a,b){this.each(function(c){c.set(a,b)})};Array.prototype.removeClass=function(a){this.each(function(b){b.removeClass(a)})};Array.prototype.setStyle=function(a,b){this.each(function(c){c.setStyle(a,b)})};
Element.clean=function(a){switch(a.get("tag")){case "select":for(var b=a.options.length-1;0<=b;b--)a.options[b]=null;break;default:a.getElements("[data-field]").each(function(a){switch(a.get("tag")){case "input":case "textarea":case "select":a.set("value","");break;default:a.set("html","")}})}};Element.CheckBox=function(a){a=$(a);a.get("checked")||(a.set("value","false"),a.set("checked",!0),a.setStyle("visibility","hidden"))};
Element.GetDataString=function(a){var b={};a.getElements("input , select , textarea").each(function(a){var c=a.get("name");null!=c&&(b[c]||(b[c]=[]),b[c].push(a.get("value")))});var c=[];Object.forEach(b,function(a,b){c.push(b+"="+a.join(","))});return c.join("&")};
Element.SelectAll=function(a,b,c){void 0==a&&(a="selectAll");a=$(a);if(null==a)return null;c||(c=1E4);if(void 0==b){var d=a.getParent("form");null==d&&(d=$(document.body));b=d.getElements("input[type=checkbox]").filter(function(a){return"ID"==a.get("name")})}b=b.filter(function(a){return!a.get("disabled")});a.addEvent("click",function(){var a=this.get("checked");b.each(function(b,d){d<c&&(b.set("checked",a),b.fireEvent("click"))})})};
Element.GetSelectedValue=function(a){null==a&&(a=$$("input[type=checkbox]").filter(function(a){return"ID"==a.get("name")}));var b=[];a.each(function(a){a.get("checked")&&b.push(a.get("value"))});return b};Element.Bind=function(a,b,c){Object.forEach(b,function(b,e){var f=a.getElement("[data-field="+(c?c+"."+e:e)+"]");null!=f&&Element.SetValue(f,b)})};
Element.SetValue=function(a,b){switch(a.get("tag")){case "input":case "textarea":a.set("value",b);break;case "select":for(var c=-1,d=0;d<a.options.length;d++)if(a.options[d].value==b){c=d;break}-1!=c&&(a.options[c].selected=!0);break;default:a.set("html",b)}};Element.GetValue=function(a){var b=null;switch(a.get("tag")){case "input":case "select":case "textarea":b=a.get("value");break;default:b=a.get("html")}return b};
Element.GetXml=function(a){a=$(a);if(null==a)return null;var b=function(a){for(var b=[],c=0;c<a.attributes.length;c++){var g=a.attributes[c];"data-node"!=g.name&&0==g.name.indexOf("data-")&&b.push(g.name+'="'+g.value+'"')}return b.join(" ")},c=[];c.push("<root>");switch(a.get("tag")){case "table":a.getElements("tbody tr").each(function(a){null==a.get("data-noxml")&&(c.push("<item "+b(a)+">"),a.getElements("[data-node]").each(function(a){c.push(["<",a.get("data-node")," ",b(a),">",Element.GetValue(a),
"</",a.get("data-node"),">"].join(""))}),c.push("</item>"))})}c.push("</root>");return c.join("")};Element.GetAttribute=function(a,b){b||(b="data-");a=$(a);if(null!=a){for(var c={},d=new RegExp("^"+b),e=0;e<a.attributes.length;e++){var f=a.attributes[e];if(d.test(f.name)){var g=f.name.substr(b.length);c[g]=f.value}}return c}};
Element.Linkage=function(a,b,c){var d=function(a){var c=[],d=!1;b.each(function(b){d&&c.push(b);b==a&&(d=!0)});return c},e=function(a){var b=[],c=a.get("data-value");null==c&&(c=0);b.push(c);d(a).each(function(a){a=a.get("data-value");null==a&&(a=0);b.push(a)});return b};b.addEvent("change",function(f){var g=this,h=g.get("value");d(g).each(function(a){Element.clean(a);a.fade("hide")});(new Request.JSON({url:a,onReuqest:function(){b.set("disabled",!0)},onComplete:function(){b.set("disabled",!1)},onSuccess:function(a){var b=
a.info;if(null==b)return!1;var e=h;d(g).each(function(a){var d=a.get("data-value");b.filter(function(a){a.ID==d&&(a.selected=!0);return a.ParentID==e}).bind(a,c);e=a.get("value").toInt();a.fade(isNaN(e)?"hide":"show")})}})).post({Value:h,DefaultValue:e(g).join(",")});b.set("disabled",!0)})};Element.ReadOnly=function(a){a=$(a);switch(a.get("tag")){case "select":a.removeEvents("change");var b=Element.GetValue(a);a.addEvent("change",function(c){Element.SetValue(a,b)})}};
Element.GetNext=function(a){var b=null;switch(a.get("tag")){case "input":case "textarea":case "select":var c=a.getParent("form");null!=c&&(c=c.getElements("input,textarea,select"),1<c.length&&(a=c.indexOf(a),a++,c.length>a&&(b=c[a])))}return b};Element.SetEmptyFocus=function(a,b){a=$(a);null!=a&&(b||(b="empty-focus"),a.addEvents({focus:function(){this.removeClass(b)},blur:function(a){""==this.get("value")&&this.addClass(b)}}),function(){a.fireEvent("blur")}.delay(1E3))};
Element.GetData=function(a){a||(a=$(document.body));a.getElements("[name]").each(function(a){a.get("name");a.get("value")});return a};Element.SetCurrent=function(a,b,c){b||(b="current");c||(c=function(a){a=a.getElement("input[type=radio]");return null==a?!1:a.get("checked")});a.each(function(a){c.apply(this,[a])?a.addClass(b):a.removeClass(b)})};
UI.getSize=function(){var a=Math.max(document.documentElement.scrollHeight,document.documentElement.clientHeight);a>window.screen.availHeight&&(a=document.documentElement.clientHeight);var b=document.documentElement.clientWidth;return{x:b,y:a,width:b,height:$(document.body).getSize().y,top:Math.max(document.body.scrollTop,document.documentElement.scrollTop)}};
UI.center=function(a,b){a=$(a);void 0==b&&(b=document.body);$(b);var c=UI.getSize(),d=a.getCoordinates();a.setStyles({left:(c.x-d.width)/2,top:0>(c.height-d.height)/2?0:Browser.ie6?(c.height-d.height)/2+c.top:(c.height-d.height)/2});return{x:(c.x-d.width)/2,y:(c.height-d.height)/2}};
UI.Sound=function(a,b){/^\w+$/i.test(a)&&(a=_gPath+"/sound/"+a+".mp3");var c=$("UI_Sound_Player"),d=!!document.createElement("video").canPlayType;null==c&&(d?(c=new Element("audio",{id:"UI_Sound_Player",autoplay:"autoplay"}),c.inject(document.body)):window.Swiff&&(c=new Swiff(_gPath+"/images/dewplayer.swf",{id:"UI_Sound_Player",width:0,height:0,param:{wmode:"transparent"},vars:{mp3:a,javascript:"on",autostart:"true"},styles:{position:"absolute",top:0,left:0,display:"none"}}),c.inject(document.body)));
if(d)c.set("src",a);else if(c&&c.dewset&&!b)try{c.dewset(a)}catch(e){c.dispose(),UI.Sound(a,e)}};UI.SoundText=function(a){UI.Sound("http://tts.baidu.com/text2audio?lan=zh&ie=UTF-8&spd=5&text="+a)};Object.get=function(a,b,c){c=void 0==c?!1:c;var d=null;Object.each(a,function(a,f){if(b==f||!c&&b.toLowerCase()==f.toLowerCase())d=a});return d};
var _importList=[],_gPath=function(){for(var a=document.getElementsByTagName("script"),b=0;b<a.length;b++){var c=a[b].src;if("/scripts/mootools.js"==c.toLowerCase().substr(c.length-20))return c.substr(0,c.length-20)}return""}();
function $import(a,b,c){void 0==c&&(c=!1);if(_importList.contains(a))if(c)$$("head script").each(function(b){null!=b.get("src")&&b.get("src").EndWith(a)&&b.dispose()});else return;_importList.push(a);void 0==b&&(b=!0);!a.contains("/")&&a.EndWith(".js")&&(a=_gPath+"/scripts/"+a);!a.contains("/")&&a.EndWith(".css")&&(a=_gPath+"/styles/"+a);c=document.getElementsByTagName("head")[0];var d=null,e=a.substring(a.lastIndexOf("."));e.StartWith(".js")?d=b?'<script language="javascript" type="text/javascript" src="'+
a+'">\x3c/script>':new Element("script",{language:"javascript",type:"text/javascript",src:a}):e.StartWith(".css")&&(d=b?'<link type="text/css" rel="Stylesheet" href="'+a+'" />':new Element("link",{type:"text/css",rel:"Stylesheet",href:a}));b?document.write(d):d.inject($(c))}
function SetHome(a,b){void 0==vrl&&(b=location.host);try{a.style.behavior="url(#default#homepage)",a.setHomePage(b)}catch(c){if(window.netscape){try{netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect")}catch(d){alert("\u62b1\u6b49\uff01\u60a8\u7684\u6d4f\u89c8\u5668\u4e0d\u652f\u6301\u76f4\u63a5\u8bbe\u4e3a\u9996\u9875\u3002\u8bf7\u5728\u6d4f\u89c8\u5668\u5730\u5740\u680f\u8f93\u5165\u201cabout:config\u201d\u5e76\u56de\u8f66\u7136\u540e\u5c06[signed.applets.codebase_principal_support]\u8bbe\u7f6e\u4e3a\u201ctrue\u201d\uff0c\u70b9\u51fb\u201c\u52a0\u5165\u6536\u85cf\u201d\u540e\u5ffd\u7565\u5b89\u5168\u63d0\u793a\uff0c\u5373\u53ef\u8bbe\u7f6e\u6210\u529f\u3002")}Components.classes["@mozilla.org/preferences-service;1"].getService(Components.interfaces.nsIPrefBranch).setCharPref("browser.startup.homepage",b)}}}
function addBookmark(a){void 0==a&&(a=document.title);var b=parent.location.href;try{window.external.addFavorite(b,a)}catch(c){try{window.sidebar.addPanel(a,b,"")}catch(d){alert("\u52a0\u5165\u6536\u85cf\u5931\u8d25\uff0c\u8bf7\u4f7f\u7528Ctrl+D\u8fdb\u884c\u6dfb\u52a0,\u6216\u624b\u52a8\u5728\u6d4f\u89c8\u5668\u91cc\u8fdb\u884c\u8bbe\u7f6e\uff01","\u63d0\u793a\u4fe1\u606f")}}}
function fixPng(a){if(Browser.ie6){var b=/^url\(\"(.*)(\.png)\"\)$/i;void 0==a&&(a=$$("body *"));a.each(function(a){var d=a.getStyle("background-image").toLowerCase();b.test(d)&&(d=b.exec(d)[1]+b.exec(d)[2],a.setStyles({"background-image":"none",filter:"progid:DXImageTransform.Microsoft.AlphaImageLoader(enabled=true, sizingMethod=scale, src='"+d+"')"}))})}}function fixTextShadow(a){}
function setIframeHeight(a){parent.document.all(a).height=parent.document.all(a).style.height=$(document.body).getStyle("height").toInt()}
var Regex={test:function(a,b){if(!Regex[a])return null;switch(typeof Regex[a]){case "function":return Regex[a].apply(this,[b]);default:return Regex[a].test(b)}},qq:/^[1-9]\d{4,9}$/,email:/^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/,phone:/^(\d{3,4}-)\d{7,8}(-\d{1,6})?$/,mobile:/^1\d{10}$/,date:/^\d{4}\-[01]?\d\-[0-3]?\d$|^[01]\d\/[0-3]\d\/\d{4}$|^\d{4}\u5e74[01]?\d\u6708[0-3]?\d[\u65e5\u53f7]$/,"int":/^\d+$/,integer:/^[1-9][0-9]*$/,number:/^[+-]?[1-9][0-9]*(\.[0-9]+)?([eE][+-][1-9][0-9]*)?$|^[+-]?0?\.[0-9]+([eE][+-][1-9][0-9]*)?$/,
numberwithzero:/^[0-9]+$/,money:/^\d+(\.\d{0,2})?$/,alpha:/^[a-zA-Z]+$/,alphanum:/^[a-zA-Z0-9_]+$/,betanum:/^[a-zA-Z0-9-_]+$/,cnid:/^\d{15}$|^\d{17}[0-9a-zA-Z]$/,urls:/^(http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?$/,url:/^(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?$/,chinese:/^[\u2E80-\uFE4F]+$/,postal:/^[0-9]{6}$/,mutiyyyymm:/(20[0-1][0-9]((0[1-9])|(1[0-2]))[\,]?)+$/,name:/^([\u4e00-\u9fa5|A-Z|\s]|\u3007)+([\.\uff0e\u00b7\u30fb]?|\u3007?)+([\u4e00-\u9fa5|A-Z|\s]|\u3007)+$/,
username:/^[a-zA-Z0-9_\u4e00-\u9fa5]{2,16}$/,password:/^.{5,16}$/,realname:/^[\u2E80-\uFE4F]{2,5}$/,passport:/^[a-z0-9]\d{7,8}$/i,company:/^\d{15}$/,idcard:function(a){if(!/^\d{6}(19|20)\d{2}[01]\d[0123]\d{4}[0-9x]$/i.test(a))return!1;for(var b=[7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2],c=0,d=0;d<a.length-1;d++)c+=a[d]*b[d];return"10X98765432".split("")[c%11].toLowerCase()==a[a.length-1].toLowerCase()}};