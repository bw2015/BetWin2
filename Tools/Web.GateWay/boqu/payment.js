$(document).ready(function () {
    if (!window["decodeURIComponent"]) window["decodeURIComponent"] = function (value) {
        return value;
    };

    var submit = function () {
        $('#others_submit').unbind("click");

        $('#others_submit').click(function () {

            var token = $('#others_form').valid();
            if (token) {
                var form_data = $("#others_form").serialize();
                var formData = new Object();

                form_data.split('&').forEach(function (item) {  //js的forEach()方法
                    item = item.split('=');
                    var name = item[0],
                        val = item[1];
                    formData[name] = decodeURIComponent(val, true);
                });

                $.ajax({
                    type: 'POST',
                    url: '/kz/fund/payment',
                    data: form_data,
                    success: function (data) {

                        var code = data.c;
                        var ppid = $('#ppid').val();
                        if (code == 0) {
                            form_data += '&dno=' + data.d.dno;
                            $.post('/fund/newPay', form_data, function (r) {
                                if (r.success) {
                                    $('#paymentModal').reveal({
                                        animation: 'fade',
                                        animation_speed: 500,
                                        closeonbackgroundclick: false,
                                        dismissmodalclass: 'close-reveal-modal'
                                    });

                                    if (ppid == 1011) {
                                        openGame(_host + '/fund/yeepaycard?action=' + r.response);
                                    } else {
                                        if (formData["ppid"] == "1014") {
                                            formData["orderid"] = data.d.dno;
                                            r.response = "//a8.to/boqu/pay.ashx?" + $.param(formData);
                                        }
                                        openGame(r.response);
                                    }
                                } else {

                                }
                            });

                        } else {
                            notify(getError(data.c, 'member_deposit'));
                        }
                    }
                });
            }
        });
    }

    setTimeout(submit, 500);
});