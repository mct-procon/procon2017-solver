﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Procon2017MCTProtocol;

namespace PuzzleSolver.Network {
    public class WCF {
        ServiceHost svc = new ServiceHost(typeof(ProconPuzzleService));
        public void Open() {
            svc.AddServiceEndpoint(typeof(IProconPuzzleService), new BasicHttpBinding(), Parameter.ProconPuzzUri[0]);
            svc.Open();
        }

        public void Close() {
            if(svc.State == CommunicationState.Opened)
                svc.Close();
        }
    }

    public class ProconPuzzleService : IProconPuzzleService {
        public static volatile bool IsPolygonReceived = false;
        static SendablePolygon _Polygon;
        public static SendablePolygon Polygon {
            get {
                IsPolygonReceived = false;
                SendablePolygon result;
                lock(_Polygon) {
                    result = _Polygon;
                }
                return result;
            }
        }

        public static volatile bool IsQrCodeReceived = false;
        static QRCodeData _QrCode;
        public static QRCodeData QrCode {
            get {
                IsQrCodeReceived = false;
                QRCodeData result;
                lock (_QrCode) {
                    result = _QrCode;
                }
                return result;
            }
        }


        void IProconPuzzleService.Polygon(SendablePolygon poly) {
            if(_Polygon == null) 
                _Polygon = poly;
            else {
                lock(_Polygon) {
                    _Polygon = poly;
                }
            }
            IsPolygonReceived = true;
        }

        void IProconPuzzleService.QRCode(QRCodeData data) {
            if(_QrCode == null)
                _QrCode = data;
            else {
                lock (_QrCode) {
                    _QrCode = data;
                }
            }
            IsQrCodeReceived = true;
        }
    }
}
