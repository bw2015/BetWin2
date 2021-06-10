// 任选5
data["五中五"] = 0;
for (var i1 = 1; i1 <= 11; i1++) {
    for (var i2 = i1 + 1; i2 <= 11; i2++) {
        for (var i3 = i2 + 1; i3 <= 11; i3++) {
            for (var i4 = i3 + 1; i4 <= 11; i4++) {
                for (var i5 = i4 + 1; i5 <= 11; i5++) {
                    total++;
                    if (i1 == 1 && i2 == 2 && i3 == 3 && i4 == 4 && i5 == 5) {
                        data["五中五"]++;
                    }
                }
            }
        }
    }
}