<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="BingAnswers.Azure" generation="1" functional="0" release="0" Id="b982ab9a-46c9-438c-85e8-926ee35c854b" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="BingAnswers.AzureGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="BingAnswers:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/LB:BingAnswers:Endpoint1" />
          </inToChannel>
        </inPort>
        <inPort name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/LB:BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="BingAnswersInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapBingAnswersInstances" />
          </maps>
        </aCS>
        <aCS name="Certificate|BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/MapCertificate|BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:BingAnswers:Endpoint1">
          <toPorts>
            <inPortMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Endpoint1" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapBingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapBingAnswersInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswersInstances" />
          </setting>
        </map>
        <map name="MapCertificate|BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="BingAnswers" generation="1" functional="0" release="0" software="D:\personal\Bing Web Answers\BingAnswers\BingAnswers.Azure\csx\Debug\roles\BingAnswers" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/SW:BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;BingAnswers&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;BingAnswers&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswersInstances" />
            <sCSPolicyFaultDomainMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswersFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyFaultDomain name="BingAnswersFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="BingAnswersInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="e7570cc8-ef3c-4047-b8a8-cc500abea6fb" ref="Microsoft.RedDog.Contract\ServiceContract\BingAnswers.AzureContract@ServiceDefinition.build">
      <interfacereferences>
        <interfaceReference Id="f73f6536-83aa-4718-8e36-0c467573b073" ref="Microsoft.RedDog.Contract\Interface\BingAnswers:Endpoint1@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers:Endpoint1" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="78cf7831-df46-4db3-8e5a-bf5b645694bc" ref="Microsoft.RedDog.Contract\Interface\BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/BingAnswers.Azure/BingAnswers.AzureGroup/BingAnswers:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>