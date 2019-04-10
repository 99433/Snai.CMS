﻿using Microsoft.Extensions.Options;
using Snai.CMS.Manage.Business.Interface;
using Snai.CMS.Manage.Common;
using Snai.CMS.Manage.Common.Encrypt;
using Snai.CMS.Manage.Common.Infrastructure;
using Snai.CMS.Manage.Common.Infrastructure.Extension;
using Snai.CMS.Manage.Common.Utils;
using Snai.CMS.Manage.DataAccess.Interface;
using Snai.CMS.Manage.Entities.BackManage;
using Snai.CMS.Manage.Entities.Settings;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Snai.CMS.Manage.Business.Implement
{
    public class CMSAdminBO : ICMSAdminBO
    {
        #region 属性声明

        IOptions<LogonSettings> LogonSettings;
        ICMSAdminDao CMSAdminDao;
        HttpContextExtension HttpExtension;
        ICMSAdminCookie CMSAdminCookie;

        #endregion

        #region 构造函数

        public CMSAdminBO(IOptions<LogonSettings> logonSettings, ICMSAdminDao cmsAdminDao, HttpContextExtension httpExtension, ICMSAdminCookie cmsAdminCookie)
        {
            LogonSettings = logonSettings;
            CMSAdminDao = cmsAdminDao;
            HttpExtension = httpExtension;
            CMSAdminCookie = cmsAdminCookie;
        }

        #endregion

        #region 账号操作

        //验证密码是否合法
        public Message VerifyPassword(string password)
        {
            var msg = new Message(0, "");

            var valPwd = Validator.IsPassword(password);

            if (valPwd == 1)
            {
                msg.Code = 105;
                msg.Msg = "密码包含空格";

                return msg;
            }
            if (valPwd == 2)
            {
                msg.Code = 106;
                msg.Msg = "密码长度不足";

                return msg;
            }
            if (valPwd == 4)
            {
                msg.Code = 107;
                msg.Msg = "密码不能为空";

                return msg;
            }
            if (valPwd == 5)
            {
                msg.Code = 108;
                msg.Msg = "密码不能是同一个字符";

                return msg;
            }
            if (valPwd == 6)
            {
                msg.Code = 109;
                msg.Msg = "密码不能是递增或递减的数字或字母";

                return msg;
            }
            if (valPwd == 7)
            {
                msg.Code = 110;
                msg.Msg = "密码属于社会工程学中字符";

                return msg;
            }
            if (valPwd == 8)
            {
                msg.Code = 111;
                msg.Msg = "密码必须是字母与(数字或特殊符号)组合";

                return msg;
            }

            return msg;
        }

        //添加账号
        public Message CreateAdmin(Admin admin)
        {
            var msg = new Message(10, "");
            if (admin == null)
            {
                msg.Code = 101;
                msg.Msg = "账号不能为空";

                return msg;
            }

            if (string.IsNullOrEmpty(admin.UserName.Trim()))
            {
                msg.Code = 102;
                msg.Msg = "用户名不能为空";

                return msg;
            }

            if (admin.UserName.Length > 32)
            {
                msg.Code = 102;
                msg.Msg = "用户名长度不能多于32个字符";

                return msg;
            }

            var uAdmin = CMSAdminDao.GetAdminByUserName(admin.UserName);
            if (uAdmin != null && uAdmin.ID > 0)
            {
                msg.Code = 11;
                msg.Msg = "添加的账号用户名已存在";

                return msg;
            }

            if (string.IsNullOrEmpty(admin.Password.Trim()) || !admin.Password.Trim().Equals(admin.RePassword))
            {
                msg.Code = 103;
                msg.Msg = "密码为空或两次密码不一致";

                return msg;
            }

            var pwdMsg = this.VerifyPassword(admin.Password);
            if (!pwdMsg.Success)
            {
                return msg;
            }

            if (admin.RoleID <= 0)
            {
                msg.Code = 104;
                msg.Msg = "请选择账号的角色";

                return msg;
            }

            admin.Password = EncryptMd5.EncryptByte(admin.Password.Trim());
            admin.CreateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
            admin.UpdateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var addState = CMSAdminDao.CreateAdmin(admin);

            if (addState)
            {
                msg.Code = 0;
                msg.Msg = "添加账号成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "添加账号失败";
            }

            return msg;
        }

        //取全部账号
        public IEnumerable<Admin> GetAdmins()
        {
            var admins = CMSAdminDao.GetAdmins();

            return admins;
        }

        //取账号
        public Admin GetAdminByID(int id)
        {
            var admin = CMSAdminDao.GetAdminByID(id);

            if (admin != null)
            {
                return admin;
            }
            else
            {
                return null;
            }
        }

        //取账号
        public Admin GetAdminByUserName(string userName)
        {
            var admin = CMSAdminDao.GetAdminByUserName(userName);

            if (admin != null)
            {
                return admin;
            }
            else
            {
                return null;
            }
        }

        //取账号
        public IEnumerable<Admin> GetAdmins(string userName, int roleID,int pageLimit,int pageIndex)
        {
            IEnumerable<Admin> adminIE = new List<Admin>();
            IList<Admin> admins = new List<Admin>();
            if (roleID <= 0 && string.IsNullOrEmpty(userName))
            {
                adminIE = CMSAdminDao.GetAdmins();
            }
            else if (roleID <= 0 && !string.IsNullOrEmpty(userName))
            {
                adminIE = CMSAdminDao.GetAdminsLikeUserName(userName);
            }
            else
            {
                adminIE = CMSAdminDao.GetAdminsByRoleID(roleID);
                if (!string.IsNullOrEmpty(userName))
                {
                    if (adminIE != null)
                    {
                        var adminList = adminIE.ToList();
                        adminIE = adminList.Where(s => s.UserName.Contains(userName));
                    }
                }
            }

            if (adminIE != null)
            {
                admins = adminIE.ToList();
            }

            if (admins == null || admins.Count() < 0)
            {
                return null;
            }

            pageIndex = pageIndex < 1 ? 1 : pageIndex;

            if (admins.Count() <= (pageIndex - 1) * pageLimit)
            {
                return null;
            }

            admins = admins.Skip((pageIndex - 1) * pageLimit).Take(pageLimit).ToList();

            foreach (var admin in admins)
            {
                var role = this.GetRoleByID(admin.RoleID);
                if (role != null)
                {
                    admin.RoleTitle = role.Title;
                }

                var timeStamp = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
                if (admin.LockTime > timeStamp)
                {
                    admin.LockState = 2;
                }
                else
                {
                    admin.LockState = 1;
                }
            }

            return admins;
        }

        //取账号数
        public int GetAdminCount(string userName, int roleID)
        {
            IEnumerable<Admin> adminIE = new List<Admin>();
            if (roleID <= 0 && string.IsNullOrEmpty(userName))
            {
                adminIE = CMSAdminDao.GetAdmins();
            }
            else if (roleID <= 0 && !string.IsNullOrEmpty(userName))
            {
                adminIE = CMSAdminDao.GetAdminsLikeUserName(userName);
            }
            else
            {
                adminIE = CMSAdminDao.GetAdminsByRoleID(roleID);
                if (!string.IsNullOrEmpty(userName))
                {
                    if (adminIE != null)
                    {
                        var adminList = adminIE.ToList();
                        adminIE = adminList.Where(s => s.UserName.Contains(userName));
                    }
                }
            }

            if (adminIE != null)
            {
                return adminIE.ToList().Count();
            }
            else
            {
                return 0;
            }
        }

        //更新账号
        public Message UpdateAdminByID(Admin admin)
        {
            var msg = new Message(10, "");

            if (string.IsNullOrEmpty(admin.UserName.Trim()))
            {
                msg.Code = 101;
                msg.Msg = "用户名不能为空";

                return msg;
            }

            if (admin.UserName.Length > 32)
            {
                msg.Code = 101;
                msg.Msg = "用户名长度不能多于32个字符";

                return msg;
            }

            var upAdmin = this.GetAdminByID(admin.ID);
            if (upAdmin == null || upAdmin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "修改的账号不存在";

                return msg;
            }

            upAdmin = this.GetAdminByUserName(admin.UserName);
            if (upAdmin != null && upAdmin.ID != admin.ID)
            {
                msg.Code = 12;
                msg.Msg = "修改的账号用户名已存在";

                return msg;
            }

            if (!string.IsNullOrEmpty(admin.Password.Trim()))
            {
                if (!admin.Password.Trim().Equals(admin.RePassword))
                {
                    msg.Code = 102;
                    msg.Msg = "两次密码不一致";

                    return msg;
                }

                var pwdMsg = this.VerifyPassword(admin.Password);
                if (!pwdMsg.Success)
                {
                    return msg;
                }

                admin.Password = EncryptMd5.EncryptByte(admin.Password.Trim());
            }

            if (admin.RoleID <= 0)
            {
                msg.Code = 103;
                msg.Msg = "请选择账号的角色";

                return msg;
            }

            admin.UpdateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UpdateAdminByID(admin.ID, admin.UserName, admin.Password, admin.State, admin.RoleID, admin.UpdateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "修改账号成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "修改账号失败";
            }

            return msg;

        }

        //修改密码
        public Message UpdatePasswordByID(int id, string oldPassword, string password, string rePassword)
        {
            var msg = new Message(10, "");

            var admin = this.GetAdminByID(id);
            if (admin == null || admin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "修改的账号不存在";

                return msg;
            }

            if (string.IsNullOrEmpty(oldPassword))
            {
                msg.Code = 101;
                msg.Msg = "旧密码不能为空";

                return msg;
            }

            oldPassword = EncryptMd5.EncryptByte(oldPassword);
            if (!oldPassword.Equals(admin.Password))
            {
                msg.Code = 12;
                msg.Msg = "旧密码输入错误";

                return msg;
            }

            if (string.IsNullOrEmpty(password.Trim()) || !password.Trim().Equals(rePassword))
            {
                msg.Code = 102;
                msg.Msg = "密码为空或两次密码不一致";

                return msg;
            }

            var pwdMsg = this.VerifyPassword(password);
            if (!pwdMsg.Success)
            {
                return msg;
            }

            password = EncryptMd5.EncryptByte(password.Trim());
            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UpdatePasswordByID(id, password, updateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "修改密码成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "修改密码失败";
            }

            return msg;
        }

        //更新状态
        public Message UpdateAdminStateByIDs(IEnumerable<int> ids, byte state)
        {
            var msg = new Message(10, "");

            if (state != 1 && state != 2)
            {
                msg.Code = 101;
                msg.Msg = "要更改的状态有误";

                return msg;
            }

            var stateDes = state == 1 ? "启用" : "禁用";

            if (ids == null || ids.Count() <= 0)
            {
                msg.Code = 101;
                msg.Msg = $"请选择要{stateDes}的管理员";

                return msg;
            }

            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UpdateAdminStateByIDs(ids, state, updateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = $"{stateDes}成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = $"{stateDes}失败";
            }

            return msg;
        }

        //更新错误登录信息
        public Message UpdateErrorLogon(int id, int errorLogonTime, int errorLogonCount)
        {
            var msg = new Message(10, "");

            var admin = this.GetAdminByID(id);
            if (admin == null || admin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "更新的账号不存在";

                return msg;
            }

            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UpdateErrorLogon(id, errorLogonTime, errorLogonCount, updateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "更新错误登录信息成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "更新错误登录信息失败";
            }

            return msg;
        }

        //锁定账号
        public Message LockAdmin(int id, int lockTime)
        {
            var msg = new Message(10, "");

            var admin = this.GetAdminByID(id);
            if (admin == null || admin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "锁定的账号不存在";

                return msg;
            }

            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.LockAdmin(id, lockTime, updateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "锁定的账号成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "锁定的账号失败";
            }

            return msg;
        }

        //解锁
        public Message UnlockByIDs(IEnumerable<int> ids)
        {
            var msg = new Message(10, "");

            if (ids == null || ids.Count() <= 0)
            {
                msg.Code = 101;
                msg.Msg = "请选择要解锁的账号";

                return msg;
            }

            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UnlockAdminByIDs(ids, updateTime);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "解锁成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "解锁失败";
            }

            return msg;
        }

        //删除账号
        public Message DeleteAdminByIDs(IEnumerable<int> ids)
        {
            var msg = new Message(10, "");

            if (ids == null || ids.Count() <= 0)
            {
                msg.Code = 101;
                msg.Msg = "请选择要删除的账号";

                return msg;
            }

            var upState = CMSAdminDao.DeleteAdminByIDs(ids);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "删除成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "删除失败";
            }

            return msg;
        }

        //更新账号登录信息
        public Message UpdateAdminLogon(int id, int lastLogonTime, string lastLogonIP)
        {
            var msg = new Message(10, "");

            var admin = this.GetAdminByID(id);
            if (admin == null || admin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "更新的账号不存在";

                return msg;
            }

            var updateTime = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);

            var upState = CMSAdminDao.UpdateAdminLogon(id, lastLogonTime, lastLogonIP);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "更新账号登录信息成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "更新账号登录信息失败";
            }

            return msg;
        }

        #endregion

        #region 账号登录

        //登录
        public Message AdminLogin(AdminLogin adminLogin)
        {
            var msg = new Message(10, "");

            if (string.IsNullOrEmpty(adminLogin.UserName) || string.IsNullOrEmpty(adminLogin.Password))
            {
                msg.Code = 101;
                msg.Msg = "用户名或密码不能为空";

                return msg;
            }

            if (adminLogin.UserName.Length > 32)
            {
                msg.Code = 101;
                msg.Msg = "用户名或密码输入错误";

                return msg;
            }

            if (string.IsNullOrEmpty(adminLogin.VerifyCode))
            {
                msg.Code = 102;
                msg.Msg = "验证码不能为空";

                return msg;
            }

            if (adminLogin.VerifyCode.Length > 6)
            {
                msg.Code = 102;
                msg.Msg = "验证码输入错误";

                return msg;
            }

            var validate = HttpExtension.EqualsSessionValue(Consts.Session_ValidateCode, adminLogin.VerifyCode);
            HttpExtension.RemoveSession(Consts.Session_ValidateCode);
            if (!validate)
            {
                msg.Code = 103;
                msg.Msg = "验证码错误";

                return msg;
            }

            var admin = this.GetAdminByUserName(adminLogin.UserName);
            if (admin == null || admin.ID <= 0)
            {
                msg.Code = 11;
                msg.Msg = "用户名或密码错误";

                return msg;
            }

            if (admin.State == 2)
            {
                msg.Code = 12;
                msg.Msg = "用户已禁用";

                return msg;
            }

            var timeStamp = (int)DateTimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
            if (admin.LockTime > timeStamp)
            {
                msg.Code = 13;
                msg.Msg = $"帐号已锁定，请{LogonSettings.Value.LockMinute}分钟后再来登录";

                return msg;
            }

            //角色是否禁用
            var role = this.GetRoleByID(admin.RoleID);
            if (role == null || role.ID <= 0 || role.State == 2)
            {
                msg.Code = 12;
                msg.Msg = "用户角色禁用，请联系管理员处理";

                return msg;
            }

            adminLogin.Password = EncryptMd5.EncryptByte(adminLogin.Password);
            if (!admin.Password.Equals(adminLogin.Password))
            {
                if (admin.ErrorLogonTime + (LogonSettings.Value.ErrorTime * 60) < timeStamp)
                {
                    admin.ErrorLogonTime = timeStamp;
                    admin.ErrorLogonCount = 1;
                }
                else
                {
                    admin.ErrorLogonCount += 1;
                }

                if (admin.ErrorLogonCount >= LogonSettings.Value.ErrorCount)
                {
                    admin.ErrorLogonTime = 0;
                    admin.ErrorLogonCount = 0;
                    admin.LockTime = timeStamp + (LogonSettings.Value.LockMinute * 60);

                    //锁定帐号
                    this.LockAdmin(admin.ID, admin.LockTime);

                    msg.Code = 14;
                    msg.Msg = $"帐号或密码在{LogonSettings.Value.ErrorTime}分钟内，错误{LogonSettings.Value.ErrorCount}次，锁定帐号{LogonSettings.Value.LockMinute}分钟";

                    return msg;
                }
                else
                {
                    //更新错误登录信息
                    this.UpdateErrorLogon(admin.ID, admin.ErrorLogonTime, admin.ErrorLogonCount);

                    msg.Code = 15;
                    msg.Msg = $"帐号或密码错误，如在{LogonSettings.Value.ErrorTime}分钟内，错误{LogonSettings.Value.ErrorCount}次，将锁定帐号{LogonSettings.Value.LockMinute}分钟";

                    return msg;
                }
            }

            admin.LastLogonTime = timeStamp;
            admin.ErrorLogonTime = 0;
            admin.ErrorLogonCount = 0;
            admin.LockTime = 0;
            admin.LastLogonIP = HttpExtension.GetUserIP();

            //更新账号登录信息
            this.UpdateAdminLogon(admin.ID, admin.LastLogonTime, admin.LastLogonIP);

            CMSAdminCookie.SetAdiminCookie(adminLogin);

            msg.Code = 0;
            msg.Msg = "登录成功";
            return msg;
        }

        //登出
        public void AdminLogout()
        {
            CMSAdminCookie.DelAdiminCookie();
        }

        //是否登录（Message.Success true 登录在线，false 离线）
        public Message VerifyAdminLogin()
        {
            var msg = new Message(10, "");
            var adminToken = CMSAdminCookie.GetAdiminCookie();
            if (adminToken == null || string.IsNullOrEmpty(adminToken.UserName))
            {
                msg.Code = 11;
                msg.Msg = "用户没有登录";

                return msg;
            }
            else
            {
                msg.Code = 0;
                msg.Msg = "用户登录在线";
                msg.Result.AdminToken = adminToken;

                return msg;
            }
        }

        #endregion

        #region 菜单

        //添加菜单
        public Message CreateModule(Module module)
        {
            var msg = new Message(10, "");
            if (module == null)
            {
                msg.Code = 101;
                msg.Msg = "菜单不能为空";

                return msg;
            }

            if (string.IsNullOrEmpty(module.Title.Trim()))
            {
                msg.Code = 102;
                msg.Msg = "菜单名不能为空";

                return msg;
            }

            var uModules = this.GetModulesByParentID(module.ParentID, 0);
            if (uModules != null)
            {
                var uModule = uModules.FirstOrDefault(s => s.Title == module.Title);
                if (uModule != null && uModule.ID > 0)
                {
                    msg.Code = 11;
                    msg.Msg = "添加的菜单名已存在";

                    return msg;
                }
            }

            if (module.Sort <= 0)
            {
                msg.Code = 103;
                msg.Msg = "菜单排序不能小于0";

                return msg;
            }

            var addState = CMSAdminDao.CreateModule(module);

            if (addState)
            {
                msg.Code = 0;
                msg.Msg = "添加菜单成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "添加菜单失败";
            }

            return msg;
        }

        //更新菜单
        public Message UpdateModule(Module module)
        {
            var msg = new Message(10, "");
            if (module == null)
            {
                msg.Code = 101;
                msg.Msg = "菜单不能为空";

                return msg;
            }

            if (string.IsNullOrEmpty(module.Title.Trim()))
            {
                msg.Code = 102;
                msg.Msg = "菜单名不能为空";

                return msg;
            }

            var uModules = this.GetModulesByParentID(module.ParentID, 0);
            if (uModules != null)
            {
                var uModule = uModules.FirstOrDefault(s => s.Title == module.Title && s.ID != module.ID);
                if (uModule != null && uModule.ID > 0)
                {
                    msg.Code = 11;
                    msg.Msg = "修改的菜单名已存在";

                    return msg;
                }
            }

            if (module.Sort <= 0)
            {
                msg.Code = 103;
                msg.Msg = "菜单排序不能小于0";

                return msg;
            }

            var addState = CMSAdminDao.UpdateModule(module);

            if (addState)
            {
                msg.Code = 0;
                msg.Msg = "修改菜单成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "修改菜单失败";
            }

            return msg;
        }

        //取菜单
        public Module GetModule(int id)
        {
            var module = CMSAdminDao.GetModule(id);
            if (module != null)
            {
                return module;
            }
            else
            {
                return null;
            }
        }

        //取菜单
        public Module GetModule(string controller, string action)
        {
            var module = CMSAdminDao.GetModule(controller, action);
            if (module != null)
            {
                return module;
            }
            else
            {
                return null;
            }
        }

        //取菜单
        public IEnumerable<Module> GetModulesByIDs(IEnumerable<int> ids, int state)
        {
            if (ids == null || ids.Count() <= 0)
            {
                return null;
            }

            return CMSAdminDao.GetModulesByIDs(ids, state).OrderBy(s => s.Sort);
        }

        //取全部菜单
        public IEnumerable<Module> GetModules(byte state)
        {
            return CMSAdminDao.GetModules(state);
        }

        //取菜单
        public IEnumerable<Module> GetModulesByParentID(int parentID,byte getSub)
        {
            List<Module> modules = new List<Module>();

            var modules1 = CMSAdminDao.GetModulesByParentID(parentID);
            if (modules1 != null)
            {
                if (getSub == 1)
                {
                    foreach (var item1 in modules1)
                    {
                        var modules2 = CMSAdminDao.GetModulesByParentID(item1.ID);
                        if (modules2 != null)
                        {
                            foreach (var item2 in modules2)
                            {
                                var modules3 = CMSAdminDao.GetModulesByParentID(item2.ID);
                                if (modules3 != null)
                                {
                                    modules.AddRange(modules3);
                                }
                            }

                            modules.AddRange(modules2);
                        }
                    }
                }

                modules.AddRange(modules1);
            }

            return modules;
        }

        //取菜单
        public IEnumerable<Module> GetModules(string title, int parentID, int pageLimit, int pageIndex)
        {
            IEnumerable<Module> moduleIE = new List<Module>();
            IList<Module> modules = new List<Module>();
            if (parentID <= 0 && string.IsNullOrEmpty(title))
            {
                moduleIE = CMSAdminDao.GetModules(0);
            }
            else if (parentID <= 0 && !string.IsNullOrEmpty(title))
            {
                moduleIE = CMSAdminDao.GetModulesLikeTitle(title);
            }
            else
            {
                moduleIE = this.GetModulesByParentID(parentID, 1);
                if (!string.IsNullOrEmpty(title))
                {
                    if (moduleIE != null)
                    {
                        var moduleList = moduleIE.ToList();
                        moduleIE = moduleList.Where(s => s.Title.Contains(title));
                    }
                }
            }

            if (moduleIE != null)
            {
                modules = moduleIE.OrderBy(s => s.ID).OrderBy(s => s.Sort).ToList();
            }

            if (modules == null || modules.Count() < 0)
            {
                return null;
            }

            pageIndex = pageIndex < 1 ? 1 : pageIndex;

            if (modules.Count() <= (pageIndex - 1) * pageLimit)
            {
                return null;
            }

            modules = modules.Skip((pageIndex - 1) * pageLimit).Take(pageLimit).ToList();

            foreach (var module in modules)
            {
                var moduleP = this.GetModule(module.ParentID);
                if (moduleP != null)
                {
                    module.ParentTitle = moduleP.Title;
                }
            }

            return modules;
        }

        //取取菜单数
        public int GetModuleCount(string title, int parentID)
        {
            IEnumerable<Module> moduleIE = new List<Module>();
            if (parentID <= 0 && string.IsNullOrEmpty(title))
            {
                moduleIE = CMSAdminDao.GetModules(0);
            }
            else if (parentID <= 0 && !string.IsNullOrEmpty(title))
            {
                moduleIE = CMSAdminDao.GetModulesLikeTitle(title);
            }
            else
            {
                moduleIE = this.GetModulesByParentID(parentID, 1);
                if (!string.IsNullOrEmpty(title))
                {
                    if (moduleIE != null)
                    {
                        var moduleList = moduleIE.ToList();
                        moduleIE = moduleList.Where(s => s.Title.Contains(title));
                    }
                }
            }

            if (moduleIE != null)
            {
                return moduleIE.ToList().Count();
            }
            else
            {
                return 0;
            }
        }

        //更新状态
        public Message UpdateModuleState(IEnumerable<int> ids, byte state)
        {
            var msg = new Message(10, "");

            if (state != 1 && state != 2)
            {
                msg.Code = 101;
                msg.Msg = "要更改的状态有误";

                return msg;
            }

            var stateDes = state == 1 ? "启用" : "禁用";

            if (ids == null || ids.Count() <= 0)
            {
                msg.Code = 101;
                msg.Msg = $"请选择要{stateDes}的菜单";

                return msg;
            }

            var upState = CMSAdminDao.UpdateModuleState(ids, state);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = $"{stateDes}成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = $"{stateDes}失败";
            }

            return msg;
        }

        //删除菜单
        public Message DeleteModule(IEnumerable<int> ids)
        {
            var msg = new Message(10, "");

            if (ids == null || ids.Count() <= 0)
            {
                msg.Code = 101;
                msg.Msg = "请选择要删除的菜单";

                return msg;
            }

            var upState = CMSAdminDao.DeleteModule(ids);

            if (upState)
            {
                msg.Code = 0;
                msg.Msg = "删除成功";
            }
            else
            {
                msg.Code = 1;
                msg.Msg = "删除失败";
            }

            return msg;
        }

        #endregion

        #region 角色

        //取角色
        public Role GetRoleByID(int id)
        {
            var role = CMSAdminDao.GetRoleByID(id);
            if (role != null)
            {
                return role;
            }
            else
            {
                return null;
            }
        }

        //取全部角色
        public IEnumerable<Role> GetRoles(byte state)
        {
            return CMSAdminDao.GetRoles(state);
        }

        //取角色
        public IEnumerable<Role> GetRoles(string title, int pageLimit, int pageIndex)
        {
            IEnumerable<Role> roleIE = new List<Role>();
            IList<Role> roles = new List<Role>();
            if (string.IsNullOrEmpty(title))
            {
                roleIE = CMSAdminDao.GetRoles(0);
            }
            else
            {
                roleIE = CMSAdminDao.GetRolesLikeTitle(title);
            }

            if (roleIE != null)
            {
                roles = roleIE.ToList();
            }

            if (roles == null || roles.Count() < 0)
            {
                return null;
            }

            pageIndex = pageIndex < 1 ? 1 : pageIndex;

            if (roles.Count() <= (pageIndex - 1) * pageLimit)
            {
                return null;
            }

            roles = roles.Skip((pageIndex - 1) * pageLimit).Take(pageLimit).ToList();

            return roles;
        }

        //取角色数
        public int GetRoleCount(string title)
        {
            IEnumerable<Role> roleIE = new List<Role>();
            if (string.IsNullOrEmpty(title))
            {
                roleIE = CMSAdminDao.GetRoles(0);
            }
            else
            {
                roleIE = CMSAdminDao.GetRolesLikeTitle(title);
            }

            if (roleIE != null)
            {
                return roleIE.ToList().Count();
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region 权限

        //取权限
        public RoleRight GetRoleRight(int roleID, int moduleID)
        {
            var roleRight = CMSAdminDao.GetRoleRight(roleID,moduleID);
            if (roleRight != null)
            {
                return roleRight;
            }
            else
            {
                return null;
            }
        }

        //取权限
        public IEnumerable<RoleRight> GetRoleRights(int roleID)
        {
            return CMSAdminDao.GetRoleRights(roleID);
        }

        //权限判断（Message.Success true 权限成功，false 权限失败）
        public Message VerifyUserRole(string UserName, string controller, string action)
        {
            var msg = new Message(10, "");

            var admin = this.GetAdminByUserName(UserName);
            if (admin == null || admin.ID <= 0 || admin.State == 2)
            {
                msg.Code = 11;
                msg.Msg = "用户已禁用";

                return msg;
            }

            var role = this.GetRoleByID(admin.RoleID);
            if (role == null || role.ID <= 0 || role.State == 2)
            {
                msg.Code = 12;
                msg.Msg = "用户角色禁用";

                return msg;
            }

            var module = this.GetModule(controller, action);
            if (module == null || module.ID <= 0 || module.State == 2)
            {
                msg.Code = 13;
                msg.Msg = "菜单禁用";

                return msg;
            }

            var roleRight = this.GetRoleRight(role.ID, module.ID);
            if (roleRight == null || roleRight.RoleID <= 0)
            {
                msg.Code = 14;
                msg.Msg = "用户没有权限";

                return msg;
            }

            msg.Code = 0;
            msg.Msg = "验证权限成功";
            return msg;
        }

        //取角色下菜单
        public IEnumerable<Module> GetModulesByRoleID(int roleID)
        {
            var roleRights = this.GetRoleRights(roleID);

            if (roleRights == null || roleRights.Count() <= 0)
            {
                return null;
            }

            var ids = roleRights.Select(s => s.ModuleID);

            return this.GetModulesByIDs(ids, 1);
        }

        //取当前菜单
        public IEnumerable<int> GetThisModuleIDs(IEnumerable<Module> modules, int moduleID)
        {
            
            if (moduleID <= 0 || modules == null)
            {
                return null;
            }

            var moduleList = modules.ToList();
            if(!moduleList.Any())
            {
                return null;
            }

            IList<int> ids = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                var module = moduleList.FirstOrDefault(s => s.ID == moduleID);
                if (module == null || module.ID == 0)
                {
                    break;
                }

                ids.Add(module.ID);
                moduleID = module.ParentID;

                if (moduleID == 0)
                {
                    break;
                }
            }

            if (ids == null || ids.Count() <= 0)
            {
                return null;
            }

            return ids.Reverse();
        }

        #endregion
    }
}
