﻿var Logon = {};

Logon.Const = {
    userName: {
        empty: "请输入用户名"
    },

    password: {
        empty: "请输入密码"
    },

    verifyCode: {
        empty: "请输入图片中的字符",
        error: "验证码输入错误",
        maxLen: 6
    },

    ajaxErr: "很抱歉，由于服务器繁忙，请您稍后再试",

    url: {
        dologon: "/Login/DoLogin",
        jumpurl: "/Home/Index"
    }
};

Logon.Form = {
    userName: null,
    password: null,
    verifyCode: null,
    imgVerify: null,
    btnLogin: null,

    inti: function () {
        this.userName = $("#userName");
        this.password = $("#password");
        this.verifyCode = $("#verifyCode");
        this.imgVerify = $("#imgVerify");
        this.btnLogin = $("#btnLogin");
    },

    focus: function (obj) {
        obj.select();
    },

    blur: function (obj) {
    },

    error: function (obj) {
        obj.addClass("layui-form-danger");
    }
};

Logon.layui = {
    form: null,
    layer: null
};

Logon.UserName = {
    check: function () {
        var strUserName = Logon.Form.userName.val();
        return Utils.String(strUserName).isNullOrEmptyTrim();
    },

    clear: function () {
        Logon.Form.userName.val("");
        Logon.Form.userName.focus();
    },

    onfocus: function () {
        Logon.Form.focus(Logon.Form.userName);
    },
    onblur: function (e) {
        Logon.Form.blur(Logon.Form.userName);
    },

    onkeydown: function () {
        if (this.check()) {
            Logon.Form.userName.focus();
        } else {
            if (Logon.Password.check()) {
                Logon.Form.password.focus();
            }
            else {
                var verifyCodeCheck = Logon.VerifyCode.check();
                if (verifyCodeCheck > 0) {
                    Logon.Form.verifyCode.focus();
                }
                else {
                    Logon.Form.btnLogin.trigger('click');
                }
            }
        }
    },

    bind: function () {
        Logon.Form.userName.bind("focus", Logon.UserName.onfocus);
        Logon.Form.userName.bind("blur", Logon.UserName.onblur);

        Logon.Form.userName.bind("keydown", function (e) {
            $.enterSubmit(e, function () { Logon.UserName.onkeydown(); });
        });

        if (this.check()) {
            Logon.Form.userName.focus();
        }
        else {
            Logon.Form.password.focus();
        }
    }
};

Logon.Password = {
    check: function () {
        var passwd = Logon.Form.password.val();
        return Utils.String(passwd).isNullOrEmptyTrim();
    },

    clear: function () {
        Logon.Form.password.val("");
        Logon.Form.password.focus();
    },

    onfocus: function () {
        Logon.Form.focus(Logon.Form.password);
    },
    onblur: function (e) {
        Logon.Form.blur(Logon.Form.password);
    },

    onkeydown: function () {
        if (this.check()) {
            Logon.Form.password.focus();
        } else {
            var verifyCodeCheck = Logon.VerifyCode.check();
            if (verifyCodeCheck > 0) {
                Logon.Form.verifyCode.focus();
            }
            else {
                Logon.Form.btnLogin.trigger('click');
            }
        }
    },

    bind: function () {
        Logon.Form.password.bind("focus", Logon.Password.onfocus);
        Logon.Form.password.bind("blur", Logon.Password.onblur);
        Logon.Form.password.bind("keydown", function (e) {
            $.enterSubmit(e, function () { Logon.Password.onkeydown(); });
        });
    }
};

Logon.VerifyCode = {
    check: function () {
        var verifyCode = Logon.Form.verifyCode.val();

        if (Utils.String(verifyCode).isNullOrEmptyTrim()) {
            return 1;
        }

        var len = Utils.String(verifyCode).byteLength();
        if (len != Logon.Const.verifyCode.maxLen) {
            return 2;
        }

        return 0;
    },
    //刷新图片
    onrefresh: function () {
        var src = Logon.Form.imgVerify.attr("src");
        Logon.Form.imgVerify.attr("src", src + "?" + Math.random());
    },

    onfocus: function (e) {
        Logon.Form.focus(Logon.Form.verifyCode);
    },
    onblur: function (e) {
        Logon.Form.blur(Logon.Form.verifyCode);
    },

    bind: function () {
        Logon.Form.verifyCode.bind("focus", Logon.VerifyCode.onfocus);
        Logon.Form.verifyCode.bind("blur", Logon.VerifyCode.onblur);
        Logon.Form.verifyCode.bind("keydown", function (e) {
            var event = e || window.event;
            if (event.keyCode == 13) {
                var verifyCodeCheck = Logon.VerifyCode.check();
                if (verifyCodeCheck > 0) {
                    Logon.Form.verifyCode.focus();
                }
                else {
                    Logon.onsubmit();
                }
            }
        });
    }
};

Logon.BtnLogin = {
    disable: function (obj) {
        obj.text("登录中...");
        $.disableButton(obj);
    },
    enable: function (obj) {
        obj.text("登 录");
        $.enableButton(obj);
    },

    bind: function () {
        Logon.Form.btnLogin.bind("click", function () {
            return Logon.onsubmit();
        });

        Logon.Form.btnLogin.bind("keypress", function (e) {
            var event = e || window.event;
            if (event.keyCode == 13) {
                Logon.onsubmit();
            }
        });
    }
};

Logon.checkInput = function () {
    if (Logon.UserName.check()) {
        Logon.Form.error(Logon.Form.userName);
        Logon.layui.layer.msg(Logon.Const.userName.empty, { icon: 2 }); 
        Logon.Form.userName.focus();
        return false;
    }

    if (Logon.Password.check()) {
        Logon.Form.error(Logon.Form.password);
        Logon.layui.layer.msg(Logon.Const.password.empty, { icon: 2 }); 
        Logon.Form.password.focus();
        return false;
    }

    var verifyCodeCheck = Logon.VerifyCode.check();
    if (verifyCodeCheck == 1) {
        Logon.Form.error(Logon.Form.verifyCode);
        Logon.layui.layer.msg(Logon.Const.verifyCode.empty, { icon: 2 }); 
        Logon.Form.verifyCode.focus();
        return false;
    }
    if (verifyCodeCheck == 2) {
        Logon.Form.error(Logon.Form.verifyCode);
        Logon.layui.layer.msg(Logon.Const.verifyCode.error, { icon: 2 });   
        Logon.Form.verifyCode.focus();
        return false;
    }

    return true;
};

Logon.onsubmit = function () {
    if (!Logon.checkInput()) {
        return false;
    }

    Logon.BtnLogin.disable(Logon.Form.btnLogin);

    //请求参数
    var params = {
        userName: Logon.Form.userName.val(),
        password: Logon.Form.password.val(),
        verifyCode: Logon.Form.verifyCode.val()
    };
    
    var ajaxUrl = Logon.Const.url.dologon;

    //发送请求
    $.ajax({
        url: ajaxUrl,
        type: "POST",
        cache: false,
        async: true,
        dataType: "json",
        data: params,
        success: function (data, textStatus) {
            if (!data.success) {
                Logon.BtnLogin.enable(Logon.Form.btnLogin);
                Logon.Password.clear();
                Logon.VerifyCode.onrefresh();
                Logon.layui.layer.msg(data.msg, { icon: 2 }); 
            } else {
                Logon.Const.url.jumpurl = Utils.String(data.msg).isNullOrEmptyTrim() ? Logon.Const.url.jumpurl : data.msg;
                Logon.layui.layer.msg("登录成功", { icon: 1 }, function () {
                    $.jump(Logon.Const.url.jumpurl);
                }); 
            }
        },
        error: function (result, status) {
            Logon.Button.enable(Logon.Form.button);

            if (status == 'timeout') {
                alert(Logon.Const.ajaxErr);
            } else if (result.responseText != "") {
                eval("exception = " + result.responseText);
                alert(exception.Message);
            }
        }
    });
};

Logon.bind = function () {
    layui.use('form', function () {
        Logon.layui.form = layui.form;
    });

    layui.use('layer', function () {
        Logon.layui.layer = layui.layer;
    });              

    Logon.Form.inti();
    Logon.UserName.bind();
    Logon.Password.bind();
    Logon.VerifyCode.bind();
    Logon.BtnLogin.bind();

    /* 整页面监听回车键提交，跟上面每个框回车键重复
    $("body").keydown(function (event) {
        var e = event || window.event; //兼容ie
        //keyCode=13是回车键 且 按钮没有聚焦
        if (e.keyCode == "13" && !window.buttonIsFocused) {
            Logon.Form.btnLogin.click();
        }
    });
    */
};

/*
 *  窗体事件
 */
$(document).ready(function (e) {
    Logon.bind();
});
