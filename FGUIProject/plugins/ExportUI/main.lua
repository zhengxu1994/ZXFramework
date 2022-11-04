local genCode = require(PluginPath..'/GenCode_CSharp')

function onPublish(handler)
    if not handler.genCode then return end
    handler.genCode = false --prevent default output
    App.consoleView:Clear()
    genCode(handler) --do it myself

end

function onDestroy()
-------do cleanup here-------
end

local inspector = {};
function inspector.create()
    inspector.panel = CS.FairyGUI.UIPackage.CreateObject("CustomInspector", "Component1")

    inspector.checkbox = inspector.panel:GetChild("export")
    inspector.checkbox.onChanged:Set(function ()
        local obj = App.activeDoc.inspectingTarget
        if inspector.checkbox.selected then
            obj.docElement:SetScriptData("toCS","true")
        else
            obj.docElement:SetScriptData("toCS","false")
        end
    end)

    inspector.checkbox2 = inspector.panel:GetChild("extension")
    inspector.checkbox2.onChanged:Set(function ()
        local obj = App.activeDoc.inspectingTarget
        if inspector.checkbox2.selected then
            obj.docElement:SetScriptData("extension","true")
        else
            obj.docElement:SetScriptData("extension","false")
        end
    end)

    inspector.checkbox3 = inspector.panel:GetChild("bindpos")
    inspector.checkbox3.onChanged:Set(function ()
        local obj = App.activeDoc.inspectingTarget
        if inspector.checkbox3.selected then
            obj.docElement:SetScriptData("bindpos","true")
        else
            obj.docElement:SetScriptData("bindpos","false")
        end
    end)
    return inspector.panel
end

function inspector.updateUI()
    local sels = App.activeDoc.inspectingTargets
    local obj = sels[0]
    if obj.parent == nil then
        inspector.checkbox.selected =  obj.scriptData:GetAttribute("toCS","false") == "true"
        inspector.checkbox2.selected =  obj.scriptData:GetAttribute("extension","false") == "true"
        inspector.checkbox3.selected =  obj.scriptData:GetAttribute("bindpos","false") == "true"
        return true
    end
    --return true if everything is ok, return false to hide the inspector
    return false
end

--Register a inspector
App.inspectorView:AddInspector(inspector, "CustomInspector", "导出脚本设置");

--Condition to show it
App.docFactory:ConnectInspector("CustomInspector", "mixed", true, false);

App.pluginManager:LoadUIPackage(PluginPath..'/CustomInspector')