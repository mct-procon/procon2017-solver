using System;
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
            svc.AddServiceEndpoint(typeof(IProconPuzzleService), new BasicHttpBinding(), Parameter.ProconPuzzUri);
            svc.Open();
        }

        public void Close() {
            if(svc.State == CommunicationState.Opened)
                svc.Close();
        }
    }

    public class ProconPuzzleService : IProconPuzzleService {
        void IProconPuzzleService.Polygon(SendablePolygon poly) {
            // ...
        }

        void IProconPuzzleService.QRCode(string code_string) {
            // ...
        }
    }
}
