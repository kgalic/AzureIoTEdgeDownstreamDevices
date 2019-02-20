# AzureIoTEdgeDownstreamDevices
Few examples how downstream devices can be authenticated in scenarios with Azure IoT Edge as a transparent gateway

# Setup

1. [Setup Azure IoT Edge on Linux Ubuntu 18.04 VM] - https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux
2. In Azure Portal, create IoT Hub and add [new Edge device] with Symmetric key as AuthenticationType - https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device-portal
	1) Copy the connection string 
3. In Azure Portal, create two [new devices] one with sas authentication type and the other one with CertificateAuthority authentication type
	1) Copy the "Sas" device connection string
4. [Create certificates] and use the deviceId from steps 2. and 3. when creating device and edge certificate - https://github.com/Azure/azure-iot-sdk-c/blob/master/tools/CACertificates/CACertificateOverview.md
   a) [Add Root certificate to IoT Hub]  and follow the steps for the certificate verification - https://docs.microsoft.com/en-us/azure/iot-dps/how-to-verify-certificates
5. Make sure to store following certificates on the Linux VM on which IoT Edge is running:
   1. azure-iot-test-only.root.ca.cert.pem - Root Certificate
   2. new-edge-device.key.pem - Edge device private certificate
   3. new-edge-device-full-chain.cert.pem - Edge full chain certificate
6. Edit config.yaml file located in folder /etc/iotedge. Since this file is write protected, it is neccessary to change the permissions. 
   sudo chmod +o+rw config.yaml
   
   To Edit the file it is possible to use following command:
   sudo nano config.yaml
   
7. Provisioning section in config.yaml should be: 
   
	provisioning:
		source: "manual"
		device_connection_string: "your_edge_device_connection_string_from_step_2a"
 
 8. Certificates section in config.yaml should look like:
 
	certificates:
		device_ca_cert: "/home/kresimir/SharedFolder/new-edge-device-full-chain.cert.pem"
		device_ca_pk: "/home/kresimir/SharedFolder/new-edge-device.key.pem"
		trusted_ca_certs: "/home/kresimir/SharedFolder/azure-iot-test-only.root.ca.cert.pem"

9.  In Azure Portal routes should be set as:
	{
		"routes": {
			"route": "FROM /* INTO $upstream"
		}
	}
	
	Follow the steps in Azure portal and finish the module deployment to the Edge device. After this routes should be updated.

10. In order to double check if everything works fine IoT Edge runtime can be restarted
	
	sudo systemctl restart iotedge
	
	and following command should return status 'Active':
	
	sudo systemctl status iotedge
	