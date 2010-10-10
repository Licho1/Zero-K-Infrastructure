﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 2.0.50727.4927.
// 
#pragma warning disable 1591

namespace Springie.PlasmaService {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="PlasmaServiceSoap", Namespace="http://planet-wars.eu/PlasmaServer/")]
    public partial class PlasmaService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback DeleteResourceOperationCompleted;
        
        private System.Threading.SendOrPostCallback DownloadFileOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetResourceDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetResourceListOperationCompleted;
        
        private System.Threading.SendOrPostCallback RegisterResourceOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public PlasmaService() {
            this.Url = global::Springie.Properties.Settings.Default.Springie_PlasmaService_PlasmaService;
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
        public event DeleteResourceCompletedEventHandler DeleteResourceCompleted;
        
        /// <remarks/>
        public event DownloadFileCompletedEventHandler DownloadFileCompleted;
        
        /// <remarks/>
        public event GetResourceDataCompletedEventHandler GetResourceDataCompleted;
        
        /// <remarks/>
        public event GetResourceListCompletedEventHandler GetResourceListCompleted;
        
        /// <remarks/>
        public event RegisterResourceCompletedEventHandler RegisterResourceCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://planet-wars.eu/PlasmaServer/DeleteResource", RequestNamespace="http://planet-wars.eu/PlasmaServer/", ResponseNamespace="http://planet-wars.eu/PlasmaServer/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ReturnValue DeleteResource(string login, string password, string internalName) {
            object[] results = this.Invoke("DeleteResource", new object[] {
                        login,
                        password,
                        internalName});
            return ((ReturnValue)(results[0]));
        }
        
        /// <remarks/>
        public void DeleteResourceAsync(string login, string password, string internalName) {
            this.DeleteResourceAsync(login, password, internalName, null);
        }
        
        /// <remarks/>
        public void DeleteResourceAsync(string login, string password, string internalName, object userState) {
            if ((this.DeleteResourceOperationCompleted == null)) {
                this.DeleteResourceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteResourceOperationCompleted);
            }
            this.InvokeAsync("DeleteResource", new object[] {
                        login,
                        password,
                        internalName}, this.DeleteResourceOperationCompleted, userState);
        }
        
        private void OnDeleteResourceOperationCompleted(object arg) {
            if ((this.DeleteResourceCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteResourceCompleted(this, new DeleteResourceCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://planet-wars.eu/PlasmaServer/DownloadFile", RequestNamespace="http://planet-wars.eu/PlasmaServer/", ResponseNamespace="http://planet-wars.eu/PlasmaServer/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool DownloadFile(string internalName, out string[] links, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] out byte[] torrent, out string[] dependencies) {
            object[] results = this.Invoke("DownloadFile", new object[] {
                        internalName});
            links = ((string[])(results[1]));
            torrent = ((byte[])(results[2]));
            dependencies = ((string[])(results[3]));
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void DownloadFileAsync(string internalName) {
            this.DownloadFileAsync(internalName, null);
        }
        
        /// <remarks/>
        public void DownloadFileAsync(string internalName, object userState) {
            if ((this.DownloadFileOperationCompleted == null)) {
                this.DownloadFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDownloadFileOperationCompleted);
            }
            this.InvokeAsync("DownloadFile", new object[] {
                        internalName}, this.DownloadFileOperationCompleted, userState);
        }
        
        private void OnDownloadFileOperationCompleted(object arg) {
            if ((this.DownloadFileCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DownloadFileCompleted(this, new DownloadFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://planet-wars.eu/PlasmaServer/GetResourceData", RequestNamespace="http://planet-wars.eu/PlasmaServer/", ResponseNamespace="http://planet-wars.eu/PlasmaServer/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ResourceData GetResourceData(string md5, string internalName) {
            object[] results = this.Invoke("GetResourceData", new object[] {
                        md5,
                        internalName});
            return ((ResourceData)(results[0]));
        }
        
        /// <remarks/>
        public void GetResourceDataAsync(string md5, string internalName) {
            this.GetResourceDataAsync(md5, internalName, null);
        }
        
        /// <remarks/>
        public void GetResourceDataAsync(string md5, string internalName, object userState) {
            if ((this.GetResourceDataOperationCompleted == null)) {
                this.GetResourceDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetResourceDataOperationCompleted);
            }
            this.InvokeAsync("GetResourceData", new object[] {
                        md5,
                        internalName}, this.GetResourceDataOperationCompleted, userState);
        }
        
        private void OnGetResourceDataOperationCompleted(object arg) {
            if ((this.GetResourceDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetResourceDataCompleted(this, new GetResourceDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://planet-wars.eu/PlasmaServer/GetResourceList", RequestNamespace="http://planet-wars.eu/PlasmaServer/", ResponseNamespace="http://planet-wars.eu/PlasmaServer/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ResourceData[] GetResourceList() {
            object[] results = this.Invoke("GetResourceList", new object[0]);
            return ((ResourceData[])(results[0]));
        }
        
        /// <remarks/>
        public void GetResourceListAsync() {
            this.GetResourceListAsync(null);
        }
        
        /// <remarks/>
        public void GetResourceListAsync(object userState) {
            if ((this.GetResourceListOperationCompleted == null)) {
                this.GetResourceListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetResourceListOperationCompleted);
            }
            this.InvokeAsync("GetResourceList", new object[0], this.GetResourceListOperationCompleted, userState);
        }
        
        private void OnGetResourceListOperationCompleted(object arg) {
            if ((this.GetResourceListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetResourceListCompleted(this, new GetResourceListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://planet-wars.eu/PlasmaServer/RegisterResource", RequestNamespace="http://planet-wars.eu/PlasmaServer/", ResponseNamespace="http://planet-wars.eu/PlasmaServer/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ReturnValue RegisterResource(int apiVersion, string springVersion, string md5, int length, ResourceType resourceType, string archiveName, string internalName, int springHash, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] serializedData, string[] dependencies, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] minimap, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] metalMap, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] heightMap, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] torrentData) {
            object[] results = this.Invoke("RegisterResource", new object[] {
                        apiVersion,
                        springVersion,
                        md5,
                        length,
                        resourceType,
                        archiveName,
                        internalName,
                        springHash,
                        serializedData,
                        dependencies,
                        minimap,
                        metalMap,
                        heightMap,
                        torrentData});
            return ((ReturnValue)(results[0]));
        }
        
        /// <remarks/>
        public void RegisterResourceAsync(int apiVersion, string springVersion, string md5, int length, ResourceType resourceType, string archiveName, string internalName, int springHash, byte[] serializedData, string[] dependencies, byte[] minimap, byte[] metalMap, byte[] heightMap, byte[] torrentData) {
            this.RegisterResourceAsync(apiVersion, springVersion, md5, length, resourceType, archiveName, internalName, springHash, serializedData, dependencies, minimap, metalMap, heightMap, torrentData, null);
        }
        
        /// <remarks/>
        public void RegisterResourceAsync(int apiVersion, string springVersion, string md5, int length, ResourceType resourceType, string archiveName, string internalName, int springHash, byte[] serializedData, string[] dependencies, byte[] minimap, byte[] metalMap, byte[] heightMap, byte[] torrentData, object userState) {
            if ((this.RegisterResourceOperationCompleted == null)) {
                this.RegisterResourceOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRegisterResourceOperationCompleted);
            }
            this.InvokeAsync("RegisterResource", new object[] {
                        apiVersion,
                        springVersion,
                        md5,
                        length,
                        resourceType,
                        archiveName,
                        internalName,
                        springHash,
                        serializedData,
                        dependencies,
                        minimap,
                        metalMap,
                        heightMap,
                        torrentData}, this.RegisterResourceOperationCompleted, userState);
        }
        
        private void OnRegisterResourceOperationCompleted(object arg) {
            if ((this.RegisterResourceCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RegisterResourceCompleted(this, new RegisterResourceCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4918")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://planet-wars.eu/PlasmaServer/")]
    public enum ReturnValue {
        
        /// <remarks/>
        Ok,
        
        /// <remarks/>
        InvalidLogin,
        
        /// <remarks/>
        ResourceNotFound,
        
        /// <remarks/>
        InternalNameAlreadyExistsWithDifferentSpringHash,
        
        /// <remarks/>
        Md5AlreadyExists,
        
        /// <remarks/>
        Md5AlreadyExistsWithDifferentName,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4918")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://planet-wars.eu/PlasmaServer/")]
    public partial class ResourceData {
        
        private string[] dependenciesField;
        
        private string internalNameField;
        
        private ResourceType resourceTypeField;
        
        private SpringHashEntry[] springHashesField;
        
        /// <remarks/>
        public string[] Dependencies {
            get {
                return this.dependenciesField;
            }
            set {
                this.dependenciesField = value;
            }
        }
        
        /// <remarks/>
        public string InternalName {
            get {
                return this.internalNameField;
            }
            set {
                this.internalNameField = value;
            }
        }
        
        /// <remarks/>
        public ResourceType ResourceType {
            get {
                return this.resourceTypeField;
            }
            set {
                this.resourceTypeField = value;
            }
        }
        
        /// <remarks/>
        public SpringHashEntry[] SpringHashes {
            get {
                return this.springHashesField;
            }
            set {
                this.springHashesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4918")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://planet-wars.eu/PlasmaServer/")]
    public enum ResourceType {
        
        /// <remarks/>
        Map,
        
        /// <remarks/>
        Mod,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4918")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://planet-wars.eu/PlasmaServer/")]
    public partial class SpringHashEntry {
        
        private int springHashField;
        
        private string springVersionField;
        
        /// <remarks/>
        public int SpringHash {
            get {
                return this.springHashField;
            }
            set {
                this.springHashField = value;
            }
        }
        
        /// <remarks/>
        public string SpringVersion {
            get {
                return this.springVersionField;
            }
            set {
                this.springVersionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    public delegate void DeleteResourceCompletedEventHandler(object sender, DeleteResourceCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DeleteResourceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DeleteResourceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ReturnValue Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ReturnValue)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    public delegate void DownloadFileCompletedEventHandler(object sender, DownloadFileCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DownloadFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DownloadFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public string[] links {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string[])(this.results[1]));
            }
        }
        
        /// <remarks/>
        public byte[] torrent {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[2]));
            }
        }
        
        /// <remarks/>
        public string[] dependencies {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string[])(this.results[3]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    public delegate void GetResourceDataCompletedEventHandler(object sender, GetResourceDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetResourceDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetResourceDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ResourceData Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ResourceData)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    public delegate void GetResourceListCompletedEventHandler(object sender, GetResourceListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetResourceListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetResourceListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ResourceData[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ResourceData[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    public delegate void RegisterResourceCompletedEventHandler(object sender, RegisterResourceCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4918")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RegisterResourceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RegisterResourceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ReturnValue Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ReturnValue)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591