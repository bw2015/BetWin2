// 和值大小
data["大"] = 0;
data["小"] = 0
data["单"] = 0;
data["双"] = 0;

for (var i1 = 1; i1 <= 10; i1++) {
    for (var i2 = 1; i2 <= 10; i2++) {
        if (i1 == i2) continue;
        total++;
        if (i1 + i2 > 11) {
            data["大"]++;
        } else {
            data["小"]++;
        }
        if ((i1 + i2) % 2 ==0) {
            data["双"]++;
        } else {
            data["单"]++;
        }
    }
}