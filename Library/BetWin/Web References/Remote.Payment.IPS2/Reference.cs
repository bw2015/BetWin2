﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.42000 版自动生成。
// 
#pragma warning disable 1591

namespace BW.Remote.Payment.IPS2 {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3190.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="WSScanSoapBinding", Namespace="http://payat.ips.com.cn/WebService/Scan")]
    public partial class WSScan : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback scanPayOperationCompleted;
        
        private System.Threading.SendOrPostCallback barCodeScanPayOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public WSScan() {
            this.Url = "http://thumbpay.e-years.com/psfp-webscan/services/scan";
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event scanPayCompletedEventHandler scanPayCompleted;
        
        /// <remarks/>
        public event barCodeScanPayCompletedEventHandler barCodeScanPayCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://payat.ips.com.cn/WebService/Scan/scanPay", RequestNamespace="http://payat.ips.com.cn/WebService/Scan", ResponseNamespace="http://payat.ips.com.cn/WebService/Scan", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("scanPayRsp")]
        public string scanPay(string scanPayReq) {
            object[] results = this.Invoke("scanPay", new object[] {
                        scanPayReq});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void scanPayAsync(string scanPayReq) {
            this.scanPayAsync(scanPayReq, null);
        }
        
        /// <remarks/>
        public void scanPayAsync(string scanPayReq, object userState) {
            if ((this.scanPayOperationCompleted == null)) {
                this.scanPayOperationCompleted = new System.Threading.SendOrPostCallback(this.OnscanPayOperationCompleted);
            }
            this.InvokeAsync("scanPay", new object[] {
                        scanPayReq}, this.scanPayOperationCompleted, userState);
        }
        
        private void OnscanPayOperationCompleted(object arg) {
            if ((this.scanPayCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.scanPayCompleted(this, new scanPayCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://payat.ips.com.cn/WebService/Scan/barCodeScanPay", RequestNamespace="http://payat.ips.com.cn/WebService/Scan", ResponseNamespace="http://payat.ips.com.cn/WebService/Scan", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public string barCodeScanPay([System.Xml.Serialization.XmlElementAttribute("barCodeScanPay")] string barCodeScanPay1) {
            object[] results = this.Invoke("barCodeScanPay", new object[] {
                        barCodeScanPay1});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void barCodeScanPayAsync(string barCodeScanPay1) {
            this.barCodeScanPayAsync(barCodeScanPay1, null);
        }
        
        /// <remarks/>
        public void barCodeScanPayAsync(string barCodeScanPay1, object userState) {
            if ((this.barCodeScanPayOperationCompleted == null)) {
                this.barCodeScanPayOperationCompleted = new System.Threading.SendOrPostCallback(this.OnbarCodeScanPayOperationCompleted);
            }
            this.InvokeAsync("barCodeScanPay", new object[] {
                        barCodeScanPay1}, this.barCodeScanPayOperationCompleted, userState);
        }
        
        private void OnbarCodeScanPayOperationCompleted(object arg) {
            if ((this.barCodeScanPayCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.barCodeScanPayCompleted(this, new barCodeScanPayCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3190.0")]
    public delegate void scanPayCompletedEventHandler(object sender, scanPayCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3190.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class scanPayCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal scanPayCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3190.0")]
    public delegate void barCodeScanPayCompletedEventHandler(object sender, barCodeScanPayCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3190.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class barCodeScanPayCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal barCodeScanPayCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591