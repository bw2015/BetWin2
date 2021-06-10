// 冠亚季
data["3"] = 0;
data["4"] = 0;
data["5"] = 0;
data["6"] = 0;

for (var i1 = 1; i1 <= 6; i1++) {
    for (var i2 = 1; i2 <= 6; i2++) {
        for (var i3 = 1; i3 <= 6; i3++) {
            total++;
            var key = (i1 + i2 + i3).toString();
            if (!data[key]) data[key] = 0;
            data[key]++;

        }
    }
}