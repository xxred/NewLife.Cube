﻿using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using XCode.Membership;

namespace NewLife.Cube
{
    /// <summary>控制器帮助类</summary>
    public static class ControllerHelper
    {
        #region Json响应
        /// <summary>返回结果并跳转</summary>
        /// <param name="data">结果。可以是错误文本、成功文本、其它结构化数据</param>
        /// <param name="url">提示信息后跳转的目标地址，[refresh]表示刷新当前页</param>
        /// <returns></returns>
        public static ActionResult JsonTips(Object data, String url = null)
        {
            var vr = new JsonResult(data)
            {
                //JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            //vr.Data = data;
            //vr.ContentType = contentType;
            //vr.ContentEncoding = contentEncoding;

            if (data is Exception ex)
                vr.Value = new { result = false, data = ex.GetTrue()?.Message, url };
            else
                vr.Value = new { result = true, data, url };

            return vr;
        }

        /// <summary>返回结果并刷新</summary>
        /// <param name="data">消息</param>
        /// <returns></returns>
        public static ActionResult JsonRefresh(Object data) => JsonTips(data, "[refresh]");
        #endregion

        /// <summary>无权访问</summary>
        /// <param name="actionContext"></param>
        /// <param name="pm"></param>
        /// <returns></returns>
        public static ActionResult NoPermission(this ActionContext actionContext, PermissionFlags pm)
        {
            var act = (ControllerActionDescriptor)actionContext.ActionDescriptor;
            //var ctrl = (ControllerActionDescriptor)act;

            var res = "[{0}/{1}]".F(act.ControllerName, act.ActionName);
            var msg = "访问资源 {0} 需要 {1} 权限".F(res, pm.GetDescription());
            LogProvider.Provider.WriteLog("访问", "拒绝", msg);

            var ctx = actionContext.HttpContext;
            var menu = ctx.Items["CurrentMenu"] as IMenu;

            var vr = new ViewResult()
            {
                ViewName = "NoPermission"
            };
            //vr.Context = filterContext;//不需要赋值Context，执行的时候会自己获取Context

            vr.ViewData =
                new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new EmptyModelMetadataProvider(),
                    actionContext.ModelState)
                {
                    ["Resource"] = res,
                    ["Permission"] = pm,
                    ["Menu"] = menu
                };

            return vr;
        }

        ///// <summary>无权访问</summary>
        ///// <param name="controller"></param>
        ///// <param name="action"></param>
        ///// <param name="pm"></param>
        ///// <returns></returns>
        //public static ActionResult NoPermission(this Controller controller, String action, PermissionFlags pm)
        //{
        //    var res = "[{0}/{1}]".F(controller.GetType().Name.TrimEnd("Controller"), action);
        //    var msg = "访问资源 {0} 需要 {1} 权限".F(res, pm.GetDescription());
        //    LogProvider.Provider.WriteLog("访问", "拒绝", msg);

        //    var ctx = controller.HttpContext;
        //    var menu = ctx.Items["CurrentMenu"] as IMenu;

        //    var vr = new ViewResult()
        //    {
        //        ViewName = "NoPermission"
        //    };
        //    vr.ViewData = controller.ViewData;
        //    vr.ViewData["Resource"] = res;
        //    vr.ViewData["Permission"] = pm;
        //    vr.ViewData["Menu"] = menu;

        //    return vr;
        //}

        /// <summary>无权访问</summary>
        /// <param name="actionContext"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static ActionResult NoPermission(this ActionContext actionContext, NoPermissionException ex)
        {
            var ctx = actionContext.HttpContext;
            var res = ctx.Request.GetEncodedUrl();
            var pm = ex.Permission;
            var msg = "无权访问数据[{0}]，没有该数据的 {1} 权限".F(res, pm.GetDescription());
            LogProvider.Provider.WriteLog("访问", "拒绝", msg);

            var menu = ctx.Items["CurrentMenu"] as IMenu;

            var vr = new ViewResult()
            {
                ViewName = "NoPermission"
            };
            vr.ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new EmptyModelMetadataProvider(),
                    actionContext.ModelState)
            {
                ["Resource"] = res,
                ["Permission"] = pm,
                ["Menu"] = menu
            };

            return vr;
        }
    }
}