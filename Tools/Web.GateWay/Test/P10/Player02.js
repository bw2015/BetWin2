// 冠亚季
data["大"] = 0;
data["小"] = 0
data["单"] = 0;
data["双"] = 0;

for (var i1 = 1; i1 <= 10; i1++) {
    for (var i2 = i1 + 1; i2 <= 10; i2++) {
        for (var i3 = i2 + 1; i3 <= 10; i3++) {
            total++;
            if ((i1 + i2 + i3) % 2 == 0) {
                data["双"]++;
            } else {
                data["单"]++;
            }
            if ((i1 + i2 + i3) > 16) {
                data["大"]++;
            } else {
                data["小"]++;
            }
        }
    }
}