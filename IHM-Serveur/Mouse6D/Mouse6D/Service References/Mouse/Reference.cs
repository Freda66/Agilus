﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mouse6D.Mouse {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://localhost/", ConfigurationName="Mouse.MouseSoap")]
    public interface MouseSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/SendMousePosition", ReplyAction="*")]
        void SendMousePosition(double tx, double ty, double tz, double rx, double ry, double rz);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/SendMousePosition", ReplyAction="*")]
        System.Threading.Tasks.Task SendMousePositionAsync(double tx, double ty, double tz, double rx, double ry, double rz);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface MouseSoapChannel : Mouse6D.Mouse.MouseSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MouseSoapClient : System.ServiceModel.ClientBase<Mouse6D.Mouse.MouseSoap>, Mouse6D.Mouse.MouseSoap {
        
        public MouseSoapClient() {
        }
        
        public MouseSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MouseSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MouseSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MouseSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void SendMousePosition(double tx, double ty, double tz, double rx, double ry, double rz) {
            base.Channel.SendMousePosition(tx, ty, tz, rx, ry, rz);
        }
        
        public System.Threading.Tasks.Task SendMousePositionAsync(double tx, double ty, double tz, double rx, double ry, double rz) {
            return base.Channel.SendMousePositionAsync(tx, ty, tz, rx, ry, rz);
        }
    }
}