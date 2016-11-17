//-----------------------------------------------------------------------------------------------------------------------------//
// Author      : solz                                                                                                          //
// Created     : 03.11.2016 20:50:03                                                                                           //
// Last Change : 03.11.2016 20:50:32                                                                                           //
// Description : <!!! Generated standard description for class IRenderEngine !!!>                                          //
//-----------------------------------------------------------------------------------------------------------------------------//

using System;

namespace WpfUserControl
{
    public interface IRenderEngine
    {
        void OnDeviceCreated(object sender, EventArgs e);
        void OnDeviceDestroyed(object sender, EventArgs e);
        void OnDeviceLost(object sender, EventArgs e);
        void OnDeviceReset(object sender, EventArgs e);
        void OnMainLoop(object sender, EventArgs e);
    }
}