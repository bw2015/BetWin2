$import("UI.Diag.js");
$import("UI.FillForm.js");

var isDrag = false;
var GroupID = 0;    // 全局变量
var t;

window.addEvent("domready", function () {

    var elem = $("canvas");
    t = new UI.WorkFlow(elem);

    // 工具栏
    $$(".Bar > ul > li > a").each(function (item) {
        var drag = item.getNext();
        if (drag == null || drag.get("tag") != "div") return;
        var position = item.getPosition();
        drag.setStyles({
            "left": position.x,
            "top": position.y
        });
        drag.setOpacity(0.01);

        new Drag.Move(drag, {
            "onStart": function (el) {
                var lnk = el.getPrevious();
                lnk.addClass("on");
            },
            "onSnap": function (el) {
                el.addClass(item.get("class"));
                el.setOpacity(1);
                isDrag = true;
            },
            "onComplete": function (el) {
                el.removeClass(item.get("class"));
                var lnk = el.getPrevious();
                var pos = el.getCoordinates();
                pos.x = pos.left + pos.width / 2;
                pos.y = pos.top + pos.height / 2;
                var position = lnk.getPosition(lnk.getParent("div.Toolbar"));
                el.setStyles({ "left": lnk.getLeft(), "top": lnk.getTop() - $(document.body).getScroll().y });
                el.setOpacity(0.01);
                lnk.removeClass("on");
                isDrag = false;
                var canvas = $("canvas").getCoordinates();
                pos.x = pos.x - canvas.left;
                pos.y = pos.y - canvas.top;
                if (pos.x > 0 && pos.x < canvas.width && pos.y > 0 && pos.y < canvas.height) {
                    var obj = t.create(JSON.decode(el.get("title")));
                    obj.GroupID = GroupID;
                    if (obj.Genre == "line") {
                        var h = 50 / Math.sqrt(2);
                        obj.Position.x1 = Math.floor(pos.x - h);
                        obj.Position.y1 = Math.floor(pos.y - h);
                        obj.Position.x2 = Math.floor(pos.x + h);
                        obj.Position.y2 = Math.floor(pos.y + h);
                    } else {
                        obj.Position.x = pos.x;
                        obj.Position.y = pos.y;
                    }
                    createElement(obj);
                }
            }
        });
    });


});

// 加载所有的组设置
function loadGroup(element) {
    element = $(element);
    new Request({
        "url": "?ac=group",
        "onSuccess": function (response) {
            var list = JSON.decode(response);
            list.each(function (item) {
                new Element("a", {
                    "href": "javascript:void(" + item.ID + ");",
                    "text": item.Name,
                    "events": {
                        "click": function (e) {
                            $("canvas").set("height", item.Setting.Height);
                            this.getParent().getElements("a.on").removeClass("on");
                            this.addClass("on");
                            this.blur();
                            loadWorkFlow(this.get("href").replace(/.*\((\d+)\).*/, "$1"));
                        }
                    }
                }).inject(element);
            });
            element.getElement("a").fireEvent("click");
        }
    }).send();
}

// 加载工作流元素
function loadWorkFlow(groupID) {
    GroupID = groupID;
    t.clear();
    new Request({
        url: "?ac=workflow&groupid=" + groupID,
        onSuccess: function (response) {
            response.split("\n").each(function (item) {
                pushList(item);
            });
            t.Draw();
        }
    }).send();

    function pushList(response) {
        var list = JSON.decode(response);
        if(list == null) return;
        list.each(function (item) {
            t.push(item);
        });
    }
}

// 添加一个元素
function createElement(item) {
    new Request({
        "url": "?ac=create",
        "method": "post",
        "data": item,
        "onSuccess": function (response) {
            var obj = JSON.decode(response);
            t.push(obj);
            t.Draw();
        }
    }).send();
}

// 修改元素信息
function updateElement(item) {
    new Request({
        "url": "?ac=update",
        "method": "post",
        "data": item,
        "onSuccess": function (response) {
            
        }
    }).send();
}

// 显示信息修改编辑框
function showEdit(item) {
    var diag = new UI.Diag({
        "type": "load",
        "title": "修改信息",
        "method": "get",
        "content": "?type=html&src=" + item.Genre + "&r=" + Math.random(),
        "width": 450,
        "height": 320,
        "onQuit": function (obj) {
            item.on = false;
        },
        "onComplete": function (obj, response) {
            var body = obj.getBody();
            new UI.Fill(body, item);

            var cancel = body.getElement("input#btnCancel");
            var save = body.getElement("input#btnSave");
            var del = body.getElementById("btnDelete");
            var type = body.getElement("input[name=Type]"); // 类路径输入框。 可能为null
            var methods = body.getElement("select[name=Methods]");  // 当前类下可选的方法。 可能为null
            var method = body.getElement("input[name=Method]"); // 选择的方法
            [cancel, save, del].each(function (obj) {
                obj.addEvents({
                    "mouseover": function (e) { this.addClass("over"); },
                    "mouseout": function (e) { this.removeClass("over"); }
                });
            });

            cancel.addEvents({
                "click": function (e) { item.on = false; obj.close(); }
            });

            save.addEvents({
                "click": function (e) {
                    this.set("disabled", true);
                    var data = UI.FillObject(body, item);
                    new Request({
                        "url": "?ac=save",
                        "method": "post",
                        "data": data,
                        "onSuccess": function (response) {
                            cancel.fireEvent("click");
                        }
                    }).send();
                }
            });

            del.addEvent("click", function () {
                if (!confirm("确认删除吗？")) return;
                var data = item;
                new Request({
                    "url": "?ac=delete",
                    "method": "post",
                    "data": data,
                    "onSuccess": function (response) {
                        item.ID = 0;
                        obj.close();
                    }
                }).send();
            });

            if (type != null) {

                type.addEvents({
                    "dblclick": function () {
                        if (this.get("value") == "") this.set("value", "SP.Aquercus.Controllers.");
                    },
                    "blur": function () {
                        var type = this.get("value");
                        if (type == "") return;
                        Element.clean(methods);
                        methods.addClass("loading");
                        methods.set("disabled", true);
                        methods.options.add(new Option("正在加载", ""));
                        new Request({
                            "url": "?type=methods&class=" + type + "&GroupID=" + GroupID,
                            "method": "get",
                            "onSuccess": function (response) {
                                methods.removeClass("loading");
                                if (!response.StartWith('[')) {
                                    Element.clean(methods);
                                    methods.options.add(new Option(response, ""));
                                    methods.set("title", response);
                                    return;
                                }
                                var list = JSON.decode(response);
                                Element.clean(methods);
                                methods.set("disabled", false);
                                methods.options.add(new Option("方法列表", ""));
                                var isMethod = false;   // 是否已经选中了方法
                                list.each(function (item) {
                                    methods.options.add(new Option(item.Name + "(" + item.Description + ")", item.Name));
                                    if (item.Name == method.get("value")) isMethod = true;
                                });
                                if (isMethod) { methods.set("value", method.get("value")); }
                            }
                        }).send();
                    }
                });

                type.fireEvent("blur");

                methods.addEvent("change", function () {
                    if (this.get("value") == "") return;
                    method.set("value", this.get("value"));
                });
            }
        }
    });
}

if (!window["UI"]) window["UI"] = new Object();

(function (ns) {

    ns.WorkFlow = new Class({
         Implements: [Events, Options],
        // 数据源
        data : new Array() ,
        dataLine : new Array() ,    //连接线
        element : null,
        context : null,
        dragObj : null ,   // 当前拖动的对象
        options : {            
           width : 100,
           height : 60 ,    // 固定宽高
           length : 60 ,    //菱形的边长
           r : 30       // 圆的半径
        },
        initialize : function(element,options){            
            var t = this;
            t.element = element;         
            if(!t.element.getContext) return;   
            t.context = t.element.getContext("2d");
            t.setOptions(options);
            t.element.addEvents({
                "mousemove" : function(e){ t.MouseMove.apply(t,[e]); } ,
                "mousedown" : function(e){ t.MouseDown.apply(t,[e]); } ,
                "mouseup" : function(e){ t.MouseUp.apply(t,[e]); } ,
                "dblclick" : function(e){  t.DblClick.apply(t,[e]); }
            });
        } ,
        MouseMove : function(e){
            var t = this;
            var position = t.element.getPosition();
            var x = e.page.x - position.x;
            var y = e.page.y - position.y;
            if(t.dragObj != null){
               
                if(t.dragObj.Genre == "line"){

                  if(t.dragObj.select == "A"){  // 选中A点
                    t.dragObj.Position.x1 = x;
                    t.dragObj.Position.y1 = y;
                  }else if(t.dragObj.select == "B"){    // 选中B点
                    t.dragObj.Position.x2 = x;
                    t.dragObj.Position.y2 = y;
                  }else{    // 移动连接线
                       // 原始长度
                       var line = Math.sqrt(Math.pow(t.dragObj.Position.x1 - t.dragObj.Position.x2,2) + Math.pow(t.dragObj.Position.y1 - t.dragObj.Position.y2,2));
                       var lineWidth = line > 200 ? 200 : line;
                       //document.title = lineWidth + "," + (t.dragObj.x1 - t.dragObj.x2);
                       // 新的长度
                       var w = Math.abs(lineWidth * (t.dragObj.Position.x1 - t.dragObj.Position.x2) / line) / 2;
                       var h = Math.abs(lineWidth * (t.dragObj.Position.y1 - t.dragObj.Position.y2) / line) / 2;
                       t.dragObj.Position.x1 = Math.floor(x + w);
                       t.dragObj.Position.y1 = Math.floor(y + h);
                       t.dragObj.Position.x2 = Math.floor(x - w);
                       t.dragObj.Position.y2 = Math.floor(y - h);
                   }

                }else{
                    t.data.filter(function(item){ return (item.Position.x1 == t.dragObj.Position.x && item.Position.y1 == t.dragObj.Position.y); }).each(function(item){
                        item.Position.x1 = x;
                        item.Position.y1 = y;
                        updateElement(item);
                    });
                    t.data.filter(function(item){ return (item.Position.x2 == t.dragObj.Position.x && item.Position.y2 == t.dragObj.Position.y); }).each(function(item){
                        item.Position.x2 = x;
                        item.Position.y2 = y;
                        updateElement(item);
                    });
                    t.dragObj.Position.x = x;
                    t.dragObj.Position.y = y;
                }
            }
            t.Draw();
        } ,
        MouseDown : function(e){
            var t = this;  
            t.data.each(function(item){ item.on = false; });
            var position = t.element.getPosition();
            var x = e.page.x - position.x;
            var y = e.page.y - position.y;
            var obj = t.Find(x,y);
            if(obj != null){
                obj.on = true;
                t.dragObj = obj;
            }
            t.Draw();
        } ,
        MouseUp : function(e){
            var t = this;
            if(t.dragObj != null){
                if(t.dragObj.Genre == "line"){
                    t.dragObj.PageID = t.dragObj.EventID = t.dragObj.ResultID = 0;
                    function setLinkID(linkObj){    // 设置连接对象
                        switch(linkObj.Genre){
                            case "page":
                                t.dragObj.PageID = linkObj.ID;
                            break;
                            case "event":
                                t.dragObj.EventID = linkObj.ID;
                            break;
                            case "result":
                                t.dragObj.ResultID = linkObj.ID;
                            break;
                        }
                    }
                    var obj = t.Find(t.dragObj.Position.x1, t.dragObj.Position.y1, false);
                    if(obj != null){
                       t.dragObj.Position.x1 = obj.Position.x;
                       t.dragObj.Position.y1 = obj.Position.y;
                       setLinkID(obj);
                    }
                    var obj = t.Find(t.dragObj.Position.x2, t.dragObj.Position.y2, false);
                    if(obj != null){
                       t.dragObj.Position.x2 = obj.Position.x;
                       t.dragObj.Position.y2 = obj.Position.y;
                       setLinkID(obj);
                    }
                }
                updateElement(t.dragObj);
                t.dragObj.on = false;
                t.dragObj = null;
                t.Draw();                
            }
        },
        DblClick : function(e){
            var t = this;
            var x = e.page.x - t.element.getLeft();
            var y = e.page.y - t.element.getTop();
            var obj = t.Find(x , y);
            if(obj == null) return;
            obj.on = true;
            showEdit(obj);
            t.Draw();
        } , 
        // 创建一个workflow对象
        create : function(obj){
            var work = { width: 100, height: 50, r : 30,  text : "", on: false };
            if(obj.type == "event") work.width = 60;
            return Object.merge(work,obj);
        } ,
        push : function(item){
            var t = this;
            if(item.Genre == "line")
                t.data.unshift(item);
            else
                t.data.push(item);
        } ,
        // 清除数据以及画布
        clear : function(){
            var t = this;
            t.data = new Array();
            t.Draw();
        } ,
        // 找到当前的控件
        Find : function(x, y, hasLine){
            if(hasLine == undefined) hasLine = true;
            var t = this;
            var obj = null;
            
            for(var i = t.data.length; i>0; i--){
                var item = t.data[i-1];
                if(obj != null) break;
                switch(item.Genre){
                    case "page":
                        if(x > item.Position.x - t.options.width / 2 && x < item.Position.x + t.options.width / 2 && y > item.Position.y - t.options.height / 2 && y < item.Position.y + t.options.height / 2){
                            obj = item;
                        }
                    break;
                    case "event":
                        var point = new Array();
                        var h = t.options.length * Math.sin(30 * Math.PI / 180);
                        var w = t.options.length * Math.sin(60 * Math.PI / 180);
                        point.push({x:item.Position.x, y:item.Position.y-h});
                        point.push({x:item.Position.x + w, y : item.Position.y});
                        point.push({x:item.Position.x, y:item.Position.y+h});
                        point.push({x:item.Position.x-w, y:item.Position.y});
                        // 左上区域
                        var tanA ;
                        if(x >= point[3].x && x <= point[1].x && y >= point[0].y && y <= point[2].y){
                            if(y < item.y)
                                tanA = Math.abs(x - point[0].x) / Math.abs(y - point[0].y); 
                            else
                                tanA =  Math.abs(x - point[2].x) / Math.abs(y - point[2].y); 
                            if(tanA <= 60/30) obj = item;
                        }
                    break;
                    case "result":
                        var width = Math.sqrt(Math.pow(x - item.Position.x,2) + Math.pow(y - item.Position.y,2));
                        if(width <= t.options.r) obj = item;
                    break;
                    case "line":
                        if(hasLine && x > Math.min(item.Position.x1,item.Position.x2) - 10 && x < Math.max(item.Position.x1,item.Position.x2) + 10 && y > Math.min(item.Position.y1,item.Position.y2) - 10 && y < Math.max(item.Position.y1,item.Position.y2) + 10){
                            var tan1 = Math.abs((item.Position.x1 - item.Position.x2) / (item.Position.y1 - item.Position.y2));
                            var tan2 = Math.abs((x - item.Position.x2) / (Math.abs(item.Position.y2 - item.Position.y1) -  Math.abs(y - item.Position.y1)));
                            if(Math.sqrt(Math.pow(x - item.Position.x1,2) + Math.pow(y - item.Position.y1,2)) < 10){
                                item.select = "A";
                                obj = item;
                            }else if(Math.sqrt(Math.pow(x - item.Position.x2,2) + Math.pow(y - item.Position.y2,2)) < 10){
                                item.select = "B";
                                obj = item;
                            }else if(Math.abs(tan1 - tan2) < tan1/10){
                                item.select = null;
                                obj = item;   
                            }else{
                                item.select = null;
                            }
                        }
                    break;
                }
            };
            return obj;
        } ,
        // 绘图
        Draw : function(){
            var t = this;
            if(t.context == null) return;
            t.context.clearRect(0,0,t.element.get("width"),t.element.get("height"));            
            t.data.each(function(item){
                if(item.ID == 0) return;
                switch(item.Genre){
                    case "page":
                        t.drawRect(item);
                    break;
                    case "event":
                        t.drawTriangle(item);
                    break;
                    case "result":
                        t.drawCircle(item);
                    break;
                    case "line":
                        t.drawLine(item);
                    break;
                }
            });
        } ,
        // 画方形图
        drawRect : function(item){
            var t = this;   
            var width = t.options.width;
            var height = t.options.height;    // 固定高宽
            t.context.fillStyle = item.on ? "#3399ff" : "#ffffff";
            t.context.strokeStyle = "#000000";
            t.context.lineWidth = 1;
            t.context.strokeRect(item.Position.x - width / 2 , item.Position.y - height / 2 ,width , height);
            t.context.fillRect(item.Position.x - width / 2 , item.Position.y - height / 2 , width , height);

            t.context.fillStyle = item.on ? "#ffffff" : "#000000";
            t.context.strokeStyle = "#000000";
            t.context.font  = "14px sans-serif";
            t.context.textAlign = "center";
            t.context.textBaseline = "middle";
            t.context.fillText(item.Name, item.Position.x , item.Position.y ,width);
        } ,
        // 绘制菱形
        drawTriangle : function(item){
            var t = this;
            var point = new Array();
            var length = t.options.length;
            var h = length * Math.sin(30 * Math.PI / 180);
            var w =length * Math.sin(60 * Math.PI / 180);

            point.push({x:item.Position.x, y:item.Position.y-h });
            point.push({x:item.Position.x + w, y : item.Position.y });
            point.push({x:item.Position.x, y:item.Position.y+h });
            point.push({x:item.Position.x-w, y:item.Position.y });

            t.context.fillStyle =  item.on ? "#3399ff" : "#ffffff";
            t.context.strokeStyle = "#000000";
            t.context.lineWidth = 1;

            t.context.beginPath();
            t.context.moveTo(point[0].x , point[0].y);
            for(var i=1;i<point.length;i++){
                t.context.lineTo(point[i].x , point[i].y); 
            }
            t.context.lineTo(point[0].x , point[0].y);

            t.context.fill();
            t.context.stroke();
            t.context.closePath();

            t.context.fillStyle = item.on ? "#ffffff" : "#000000";
            t.context.strokeStyle = "#000000";
            t.context.font  = "14px sans-serif";
            t.context.textAlign = "center";
            t.context.textBaseline = "middle";
            t.context.fillText(item.Name, item.Position.x  , item.Position.y ,length);
        } ,
        // 画圆形
        drawCircle : function(item){
            var t = this;
            t.context.strokeStyle = "#000000";
            t.context.fillStyle = item.on ? "#3399ff" : "#ffffff";
            t.context.beginPath();
            t.context.arc(item.Position.x,item.Position.y,t.options.r,0,Math.PI*2,true);
            t.context.stroke();
            t.context.fill();
            t.context.stroke();
            t.context.closePath();

            t.context.fillStyle = item.on ? "#ffffff" : "#000000";
            t.context.strokeStyle = "#000000";
            t.context.font  = "9px Verdana";
            t.context.textAlign = "center";
            t.context.textBaseline = "middle";
            t.context.fillText(item.Name, item.Position.x , item.Position.y , t.options.r * Math.sqrt(2));
        } ,
        // 绘制连接线
        drawLine : function(item){
            var t = this;
            t.context.strokeStyle = item.on ? "#FF0000" : "#000000";
            t.context.lineWidth = item.on ? 3 : 1;
            t.context.beginPath();
            t.context.moveTo(item.Position.x1, item.Position.y1);
            t.context.lineTo(item.Position.x2, item.Position.y2);
            t.context.fill();
            t.context.stroke();
            t.context.closePath();
        } 
    });

})(UI);