// 和值大小


for (var i1 = 1; i1 <= 11; i1++) {
    for (var i2 = i1 + 1; i2 <= 11; i2++) {
        for (var i3 = i2 + 1; i3 <= 11; i3++) {
            for (var i4 = i3 + 1; i4 <= 11; i4++) {
                for (var i5 = i4 + 1; i5 <= 11; i5++) {
                    total++;
                    var result = {
                        "单": 0, "双": 0
                    };
                    result[i1 % 2 == 0 ? "双" : "单"]++;
                    result[i2 % 2 == 0 ? "双" : "单"]++;
                    result[i3 % 2 == 0 ? "双" : "单"]++;
                    result[i4 % 2 == 0 ? "双" : "单"]++;
                    result[i5 % 2 == 0 ? "双" : "单"]++;

                    var type = result["单"] + "单" + result["双"] + "双";

                    if (!data[type]) data[type] = 0;
                    data[type]++;
                }
            }
        }
    }
}