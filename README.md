# Mtd.Kiosk.Annunciator

## Environment Variables:
### Linux:
```bash
export Mtd_Kiosk_Annunciator_Service_AzureAnnunciator__SubscriptionKey="{subscription key}"
export Mtd_Kiosk_Annunciator_Service_AzureAnnunciator__ServiceRegion="eastus"
export Mtd_Kiosk_Annunciator_Service_Kiosk__Id="{kiosk id}"
export Mtd_Kiosk_Annunciator_Service_Kiosk__Name="{kiosk name}"
export Mtd_Kiosk_Annunciator_Service_Serilog__WriteTo__0__Name="Seq"
export Mtd_Kiosk_Annunciator_Service_Serilog__WriteTo__0__Args__ServerUrl="https://seq.mtd.org/"
export Mtd_Kiosk_Annunciator_Service_RealTimeClient__HeartbeatAddressTemplate="https://kiosk.mtd.org/umbraco/api/health/buttonheartbeat?id={0}"
export Mtd_Kiosk_Annunciator_Service_RealTimeClient__RealTimeAddressTemplate="https://kiosk.mtd.org//umbraco/api/realtime/getdepartures?id={0}&log=false"
export Mtd_Kiosk_Annunciator_Service_AzureAnnunciator__SpeakerOutputDevice="sysdefault:CARD=Headphones"
```