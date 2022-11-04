table.indexof = function ( t,key )
    for k,v in pairs(t) do
        if v == key then
            return k
        end
    end
end
local function isDefaultName( name,pattern )
    if string.find( name, pattern) then
        return true
    end
end
local function startWith(str, value)
    if string.find(str, value) == 1 then
        return true
    end
end
local function endWith(str, value)
    if string.find(str, value) == #str - #value + 1 then
        return true
    end
end
local function checkPrefix(varName, memberType, memberClass)
    if not startWith(varName, "ctr_") and memberType == "Controller" then
        varName = string.gsub(varName, "_ctr", "")
        varName = "ctr_" .. varName
    end
    if not startWith(varName, "tran_") and memberType == "Transition" then
        varName = string.gsub(varName, "_tran", "")
        varName = "tran_" .. string.gsub(varName, "transition", "")
    end
    if not startWith(varName, "btn_") and memberType == "GButton" then
        if memberClass and memberClass == "ItemIcon" then
            varName = string.gsub(varName, "_item", "")
            varName = string.gsub(varName, "item_", "")
            varName = string.gsub(varName, "icon_", "")
            varName = string.gsub(varName, "_icon", "")
            varName = string.gsub(varName, "icon", "")
            if varName == "" then
                varName = "item"
            end
            if varName ~= "item" then
                varName = "item_" .. varName
            end
        else
            varName = "btn_" .. string.gsub(varName, "button", "")
        end
    end
    if not startWith(varName, "txt_") and (memberType == "GTextField" or memberType == "GRichTextField") then
        varName = string.gsub(varName, "label_", "")
        varName = string.gsub(varName, "label", "")
        varName = string.gsub(varName, "labl_", "")
        varName = string.gsub(varName, "labl", "")
        varName = string.gsub(varName, "text_", "")
        if varName ~= "text" then
            varName = string.gsub(varName, "text", "")
        end
        varName = "txt_" .. string.gsub(varName, "txt", "")
    end
    if not startWith(varName, "img_") and memberType == "GImage" then
        varName = string.gsub(varName, "image_", "")
        varName = string.gsub(varName, "image", "")
        varName = "img_" .. string.gsub(varName, "img", "")
    end
    if not startWith(varName, "loader_") and memberType == "GLoader" then
        varName = "loader_" .. string.gsub(varName, "loader", "")
    end
    if not startWith(varName, "list_") and memberType == "GList" then
        varName = "list_" .. string.gsub(varName, "list", "")
    end
    if not startWith(varName, "label_") and memberType == "GLabel" then
        varName = string.gsub(varName, "label_", "")
        varName = string.gsub(varName, "label", "")
        varName = string.gsub(varName, "labl", "")
        if varName == "" then
            varName = "label"
        end
        if varName ~= "label" then
            varName = "label_" .. varName
        end
    end
    if not startWith(varName, "graph_") and memberType == "GGraph" then
        varName = "graph_" .. string.gsub(varName, "graph", "")
    end
    if not startWith(varName, "group_") and memberType == "GGroup" then
        varName = "group_" .. string.gsub(varName, "group", "")
    end
    if not startWith(varName, "progress_") and memberType == "GProgressBar" then
        varName = string.gsub(varName, "bar_", "")
        varName = string.gsub(varName, "bar", "")
        varName = "progress_" .. string.gsub(varName, "progress", "")
    end
    if not startWith(varName, "combo_") and memberType == "GComboBox" then
        varName = string.gsub(varName, "comboBox", "")
        varName = "combo_" .. string.gsub(varName, "combo", "")
    end
    if not startWith(varName, "slider_") and memberType == "GSlider" then
        varName = "slider_" .. string.gsub(varName, "slider", "")
    end
    if not startWith(varName, "input_") and memberType == "GTextInput" then
        varName = string.gsub(varName, "txt_", "")
        varName = string.gsub(varName, "txt", "")
        varName = "input_" .. string.gsub(varName, "input", "")
    end
    if not startWith(varName, "clip_") and memberType == "GMovieClip" then
        varName = "clip_" .. string.gsub(varName, "movieClip", "")
    end
    return varName
end

local unExportItem={
    "closeButton",
    "button",
    "icon",
}

local function hasChinese(str)
    for z=1,#str do
        curByte = string.byte(str, z)
        if curByte > 127 then
            return true
        end
    end
    return false
end


local function getScriptData(handler)
    local data = {}
    local items = handler.items
    for k,v in pairs(items) do
        local scriptdata = handler:GetScriptData(v)
        if scriptdata then
            if scriptdata:GetAttribute("toCS", "false") == "true" then
                data[v.name]  = {
                    toCS = scriptdata:GetAttribute("toCS", "false") == "true", 
                    extension = scriptdata:GetAttribute("extension", "false") == "true",
                    bindpos = scriptdata:GetAttribute("bindpos", "false") == "true",
                    }
            end
        end
    end
    return data
end

local function genCode(handler)
    local settings = handler.project:GetSettings("Publish").codeGeneration
    local codePkgName = handler:ToFilename(handler.pkg.name); --convert chinese to pinyin, remove special chars etc.
    local exportCodePath = handler.exportCodePath..'/'..codePkgName
    local namespaceName = codePkgName

    if settings.packageName~=nil and settings.packageName~='' then
        namespaceName = settings.packageName
    end

    --CollectClasses(stripeMemeber, stripeClass, fguiNamespace)
    local classes = handler:CollectClasses(settings.ignoreNoname, settings.ignoreNoname, nil)
    handler:SetupCodeFolder(exportCodePath, "cs") --check if target folder exists, and delete old files

    local getMemberByName = settings.getMemberByName
    getMemberByName = false

    local genTable = getScriptData(handler)

    local classInfo
    local classCnt = classes.Count
    local classTypes = {}
    for i=0,classCnt-1 do
        classInfo = classes[i]
        classTypes[classInfo.className] = classInfo.superClassName--组件--组件类型
    end
    local writer = CodeWriter.new()
    local curByte
    local members
    local memberInfo
    local memberCnt
    local memberType
    local varName
    local asType
    local memberName
    local memberIndex
    local exportItems
    local orderItems
    local m,l,n
    local memNum
    local added
    local totalnum
    local url
    local memberClass
    local xml
    local enumerator
    local assetFiles = {}
    local elements
    local contr
    local pages
    local page
    local ctrName
    local ctrNum
    local ctrIdxs
    local extension
    local bindpos

    local items = handler.items
    for i=0,items.Count-1 do
        -- fprint(items[i].name)
        -- fprint(items[i].file)
        assetFiles[items[i].name] = items[i].file
    end

    for i=0,classCnt-1 do
        exportItems = {}
        orderItems = {}
        classInfo = classes[i]
        ctrNum = 0
        ctrIdxs = {}
        extension = false
        bindpos = false

        if genTable[classInfo.className] and not hasChinese(classInfo.className) then
            -- fprint(string.format( "正在生成 【%s】 脚本 ",classInfo.className))
            members = classInfo.members
            extension = genTable[classInfo.className].extension
            bindpos = genTable[classInfo.className].bindpos

            if classInfo.className == codePkgName or classInfo.className == codePkgName.."UI" then
                bindpos = true
            end

            writer:reset()

            writer:writeln('using FairyGUI;')
            writer:writeln()
            writer:writeln('namespace %s', namespaceName)
            writer:startBlock()
            -- url = '[Ext(ExtUrl = "url", ExtType = ObjectType.OT)]'
            -- url = string.gsub(url, "url", "ui://"..codePkgName.."/"..classInfo.className)
            -- url = string.gsub(url, "OT", string.sub(classInfo.superClassName, 2))
            writer:writeln("partial class ExtensionList")
            writer:startBlock()
            url = 'public ExtInfo e_%s = new ExtInfo() { ExtClass = typeof(%s), ExtUrl = "%s", ExtType = ObjectType.%s };'
            writer:writeln(url, classInfo.className, classInfo.className, "ui://"..codePkgName.."/"..classInfo.className, string.sub(classInfo.superClassName, 2))
            writer:endBlock()
            if extension then
                writer:writeln('partial class %s : ViewBase', classInfo.className)
            else
                writer:writeln('partial class %s', classInfo.className)
            end
            writer:startBlock()
            writer:writeln('public static readonly string Url = "%s";', "ui://"..codePkgName.."/"..classInfo.className);
            writer:writeln()
            if extension then
                -- writer:writeln('public %s(GComponent UIObj) : base(UIObj) { }', classInfo.className)
                writer:writeln('public override void OnCreate() { }', classInfo.className)
            end

            if assetFiles[classInfo.resName] then
                -- fprint(classInfo.resName)
                xml = CS.FairyEditor.XMLExtension.Load(assetFiles[classInfo.resName])
                elements = xml:Elements("controller")
                if elements then
                    -- fprint(elements.Count)
                    if elements.Count > 0 then
                        for e=0,elements.Count-1 do
                            -- fprint(elements.rawList[e]:GetAttribute("name"))
                            -- fprint(CS.FairyEditor.FController.New)
                            contr = CS.FairyEditor.FController()
                            contr:Read(elements.rawList[e])
                            pages = contr:GetPages()
                            ctrName = contr.name
                            if not isDefaultName(ctrName,"c%d+") and not table.indexof(unExportItem, ctrName) then
                                ctrName = string.gsub(ctrName, "ctr_", "")
                                ctrName = string.gsub(ctrName, "_ctr", "")
                                if ctrName ~= "ctr" then
                                    ctrName = string.gsub(ctrName, "ctr", "")
                                end
                                -- fprint(string.sub(ctrName,1,1))
                                ctrName = string.upper(string.sub(ctrName,1,1))..string.sub(ctrName, 2)
                                writer:writeln('public enum Ctr'..ctrName.."Id")
                                writer:startBlock()

                                for p=0,pages.Count-1 do
                                    page = pages[p]
                                    if page.name and page.name ~= "" and tonumber(page.name) == nil and not hasChinese(page.name) then
                                        writer:writeln(page.name.." = "..p..",")
                                    else
                                        if tonumber(ctrName) == nil then
                                            writer:writeln(string.lower(string.sub(ctrName,1,1))..string.sub(ctrName, 2)..p.." = "..p..",")
                                        end
                                    end
                                end

                                writer:endBlock()
                                writer:writeln()

                                ctrName = "Ctr"..ctrName

                                varName = checkPrefix(contr.name, "Controller")
                                ctrIdxs[varName] = "cc"..ctrNum
                                ctrNum = ctrNum + 1

                                writer:writeln("public "..ctrName.."Id "..ctrName)
                                writer:startBlock()
                                writer:writeln("get => ("..ctrName.."Id)"..ctrIdxs[varName]..".selectedIndex;")
                                writer:writeln("set => "..ctrIdxs[varName]..".selectedIndex = (int)value;")
                                writer:endBlock()
                                writer:writeln()

                                ctrName = string.gsub(varName, "ctr_", "")
                                ctrName = string.upper(string.sub(ctrName,1,1))..string.sub(ctrName, 2)
                                ctrName = "ctr"..ctrName

                                writer:writeln("public EventListener %sChanged { get => %s.onChanged; }", ctrName, ctrIdxs[varName])
                                writer:writeln()
                                writer:writeln("public string %sSelectedPage { get => %s.selectedPage; }", ctrName, ctrIdxs[varName])
                                writer:writeln()
                            end
                        end
                    end
                end
            end

            memberCnt = members.Count
            for j=0,memberCnt-1 do
                memberInfo = members[j]
                if not table.indexof(unExportItem,memberInfo.varName) then
                    memberType = classTypes[memberInfo.type] and classTypes[memberInfo.type] or memberInfo.type
                    varName = memberInfo.varName
                    memberIndex = memberInfo.index
                    memberName = memberInfo.name
                    memberClass = nil
                    if memberInfo.group==0 then
                        if memberInfo.res then
                            if not hasChinese(memberInfo.res.name) then
                                xml = CS.FairyEditor.XMLExtension.Load(memberInfo.res.file)
                                if xml:GetNode("scriptData") then
                                    if xml:GetNode("scriptData"):GetAttribute("toCS", "false") == "true" then
                                        -- fprint(memberInfo.res.name)
                                        memberClass = memberInfo.res.name
                                    end
                                end
                            end
                        end
                        m = string.gmatch(varName,"(.-)(%d+)$")
                        l,n = m()
                        if l~=nil and n~=nil then
                            -- fprint(n)
                            if string.sub(l, #l, #l) == "_" then
                                l = string.sub(l, 1, #l - 1)
                            end
                            if orderItems[l] == nil then
                                orderItems[l] = {}
                            end
                            orderItems[l][tonumber(n)+1] = {varName = checkPrefix(l, memberType, memberClass), memberClass = memberClass, fvarName = checkPrefix(varName, memberType, memberClass), memberType = memberType, index = memberIndex, name = memberName}
                        else
                            table.insert(exportItems, {varName = checkPrefix(varName, memberType, memberClass), memberClass = memberClass, memberType = memberType, index = memberIndex, name = memberName})
                        end
                    elseif memberInfo.group==1 then
                        if not isDefaultName(varName,"c%d+") then
                            table.insert(exportItems, {varName = checkPrefix(varName, "Controller"), memberType = "Controller", index = memberIndex, name = memberName})
                        end
                    else
                        if not isDefaultName(varName,"t%d+") then
                            table.insert(exportItems, {varName = checkPrefix(varName, "Transition"), memberType = "Transition", index = memberIndex, name = memberName})
                        end
                    end
                end
            end
            for k,v in pairs(orderItems) do
                added = true
                totalnum = 0
                for _k,_v in pairs(v) do
                    totalnum = totalnum + 1
                end
                -- fprint(totalnum)
                for _i=1,totalnum do
                    if v[_i] == nil then
                        fprint("组件：" ..classInfo.className .. "中的：" .. k .. " 不是顺序的!!!")
                        added = false
                    end
                end
                if added == true then
                    -- fprint("控件" .. k .. " 是顺序的!!!")
                    varName = v[1].varName
                    memberType = v[1].memberType
                    memberClass = v[1].memberClass
                    table.insert(exportItems, {varName = varName, memberClass = memberClass, memberType = memberType, memNum = totalnum, items = v})
                else
                    for _k,_v in pairs(v) do
                        table.insert(exportItems, {varName = _v.fvarName, memberClass = _v.memberClass, memberType = _v.memberType, index = _v.index, name = _v.name})
                    end
                end
            end

            table.sort(exportItems, function(a , b)
                if a == nil or b == nil then
                    return false
                end
                -- fprint(a.varName)
                return a.varName < b.varName
            end)

            for j=1,#exportItems do
                memberType = exportItems[j].memberType
                memberClass = exportItems[j].memberClass
                if memberClass then
                    exportItems[j].varName = string.gsub(exportItems[j].varName, "com_", "")
                end
                varName = exportItems[j].varName
                memNum = exportItems[j].memNum
                if memNum ~= nil and memNum>0 then
                    if memberClass then
                        writer:writeln('public %s[] %s { get; private set; }', memberClass, varName)
                    else
                        writer:writeln('public %s[] %s { get; private set; }', memberType, varName)
                    end
                else
                    memberIndex = exportItems[j].index
                    memberName = exportItems[j].name
                    if memberClass then
                        writer:writeln('public %s %s { get; private set; }', memberClass, varName)
                    else
                        if memberType == "Controller" then
                            writer:writeln('private %s %s { get; set; }', memberType, ctrIdxs[varName])
                        else
                            writer:writeln('public %s %s { get; private set; }', memberType, varName)
                        end
                        -- if memberType == "Controller" then
                        --     if getMemberByName then
                        --         writer:writeln('private %s %s { get => UIObj.GetController("%s"); }', memberType, ctrIdxs[varName], memberName)
                        --     else
                        --         writer:writeln('private %s %s { get => UIObj.GetControllerAt(%s); }', memberType, ctrIdxs[varName], memberIndex)
                        --     end
                        -- elseif memberType == "Transition" then
                        --     if getMemberByName then
                        --         writer:writeln('public %s %s { get => UIObj.GetTransition("%s"); }', memberType, varName, memberName)
                        --     else
                        --         writer:writeln('public %s %s { get => UIObj.GetTransitionAt(%s); }', memberType, varName, memberIndex)
                        --     end
                        -- else
                        --     asType = string.sub(memberType, 2)
                        --     asType = string.gsub(asType, "Component", "Com")
                        --     asType = string.gsub(asType, "ProgressBar", "Progress")
                        --     if getMemberByName then
                        --         writer:writeln('public %s %s { get => UIObj.GetChild("%s").as%s; }', memberType, varName, memberName, asType)
                        --     else
                        --         writer:writeln('public %s %s { get => UIObj.GetChildAt(%s).as%s; }', memberType, varName, memberIndex, asType)
                        --     end
                        -- end
                    end
                end
            end
            writer:writeln()

            if bindpos then
                for j=1,#exportItems do
                    memberType = exportItems[j].memberType
                    varName = exportItems[j].varName
                    memNum = exportItems[j].memNum
                    if memberType ~= "Controller" and memberType ~= "Transition" then
                        if memNum ~= nil and memNum>0 then
                            -- writer:writeln('public float[] %s_x { get; private set; }', varName)
                            -- writer:writeln('public float[] %s_y { get; private set; }', varName)
                            -- if memberType == "GList" or memberType == "GLoader" then
                            --     writer:writeln('public float[] %s_width { get; private set; }', varName)
                            --     writer:writeln('public float[] %s_height { get; private set; }', varName)
                            -- end
                        else
                            writer:writeln('public float %s_x { get; private set; } = 0;', varName)
                            writer:writeln('public float %s_y { get; private set; } = 0;', varName)
                            if memberType == "GList" or memberType == "GLoader" then
                                writer:writeln('public float %s_width { get; private set; } = 0;', varName)
                                writer:writeln('public float %s_height { get; private set; } = 0;', varName)
                            end
                        end
                        writer:writeln()
                    end
                end
            end

            writer:writeln('public override void AutoBinderUI()')
            writer:startBlock()
            for j=1,#exportItems do
                memNum = exportItems[j].memNum
                if memNum ~= nil and memNum>0 then
                    memberType = exportItems[j].memberType
                    varName = exportItems[j].varName
                    memberClass = exportItems[j].memberClass

                    if memberClass then
                        writer:writeln('%s = new %s[%d];',varName, memberClass, memNum)
                    else
                        writer:writeln('%s = new %s[%d];',varName, memberType, memNum)
                    end

                    orderItems = exportItems[j].items
                    if memberType == "Controller" then
                        -- for k=1,#orderItems do
                        --     memberIndex = orderItems[k].index
                        --     memberName = orderItems[k].name
                        --     if getMemberByName then
                        --         writer:writeln('%s[%d] = UIObj.GetController("%s");',ctrIdxs[varName], k-1, memberName)
                        --     else
                        --         writer:writeln('%s[%d] = UIObj.GetControllerAt(%s);',ctrIdxs[varName], k-1, memberIndex)
                        --     end
                        -- end
                    elseif memberType == "Transition" then
                        -- for k=1,#orderItems do
                        --     memberIndex = orderItems[k].index
                        --     memberName = orderItems[k].name
                        --     if getMemberByName then
                        --         writer:writeln('%s[%d] = UIObj.GetTransition("%s");',varName, k-1, memberName)
                        --     else
                        --         writer:writeln('%s[%d] = UIObj.GetTransitionAt(%s);',varName, k-1, memberIndex)
                        --     end
                        -- end
                    else
                        asType = string.sub(memberType, 2)
                        asType = string.gsub(asType, "Component", "Com")
                        asType = string.gsub(asType, "ProgressBar", "Progress")
                        for k=1,#orderItems do
                            memberIndex = orderItems[k].index
                            memberName = orderItems[k].name
                            if memberClass then
                                if getMemberByName then
                                    -- writer:writeln('%s[%d] = UIObj.GetChild("%s").as%s.data as %s;',varName, k-1, memberName, asType, memberClass)
                                    writer:writeln('%s[%d] = UIObj.GetChild("%s").data as %s;',varName, k-1, memberName, memberClass)
                                else
                                    -- writer:writeln('%s[%d] = UIObj.GetChildAt(%s).as%s.data as %s;',varName, k-1, memberIndex, asType, memberClass)
                                    writer:writeln('%s[%d] = UIObj.GetChildAt(%s).data as %s;',varName, k-1, memberIndex, memberClass)
                                end
                            else
                                if getMemberByName then
                                    -- writer:writeln('%s[%d] = UIObj.GetChild("%s").as%s;',varName, k-1, memberName, asType)
                                    writer:writeln('%s[%d] = UIObj.GetChild<%s>("%s");',varName, k-1, memberType, memberName)
                                else
                                    -- writer:writeln('%s[%d] = UIObj.GetChildAt(%s).as%s;',varName, k-1, memberIndex, asType)
                                    writer:writeln('%s[%d] = UIObj.GetChildAt<%s>(%s);',varName, k-1, memberType, memberIndex)
                                end
                            end
                        end
                    end
                else
                    memberType = exportItems[j].memberType
                    varName = exportItems[j].varName
                    memberIndex = exportItems[j].index
                    memberName = exportItems[j].name
                    memberClass = exportItems[j].memberClass

                    if memberType == "Controller" then
                        if getMemberByName then
                            writer:writeln('%s = UIObj.GetController("%s");',ctrIdxs[varName], memberName)
                        else
                            writer:writeln('%s = UIObj.GetControllerAt(%s);',ctrIdxs[varName], memberIndex)
                        end
                    elseif memberType == "Transition" then
                        if getMemberByName then
                            writer:writeln('%s = UIObj.GetTransition("%s");',varName, memberName)
                        else
                            writer:writeln('%s = UIObj.GetTransitionAt(%s);',varName, memberIndex)
                        end
                    else
                        asType = string.sub(memberType, 2)
                        asType = string.gsub(asType, "Component", "Com")
                        asType = string.gsub(asType, "ProgressBar", "Progress")
                        if memberClass then
                            if getMemberByName then
                                -- writer:writeln('%s = UIObj.GetChild("%s").as%s.data as %s;',varName, memberName, asType, memberClass)
                                writer:writeln('%s = UIObj.GetChild("%s").data as %s;',varName, memberName, memberClass)
                            else
                                -- writer:writeln('%s = UIObj.GetChildAt(%s).as%s.data as %s;',varName, memberIndex, asType, memberClass)
                                writer:writeln('%s = UIObj.GetChildAt(%s).data as %s;',varName, memberIndex, memberClass)
                            end
                        else
                            if getMemberByName then
                                -- writer:writeln('%s = UIObj.GetChild("%s").as%s;',varName, memberName, asType)
                                writer:writeln('%s = UIObj.GetChild<%s>("%s");',varName, memberType, memberName)
                            else
                                -- writer:writeln('%s = UIObj.GetChildAt(%s).as%s;',varName, memberIndex, asType)
                                writer:writeln('%s = UIObj.GetChildAt<%s>(%s);',varName, memberType, memberIndex)
                            end
                        end
                    end
                end
            end
            writer:endBlock() --method
            writer:writeln()

            if bindpos then
                writer:writeln('public override void InitObjPosition()')
                writer:startBlock()
                for j=1,#exportItems do
                    memberType = exportItems[j].memberType
                    varName = exportItems[j].varName
                    memNum = exportItems[j].memNum
                    memberClass = exportItems[j].memberClass
                    memberName = exportItems[j].name
                    if memberType ~= "Controller" and memberType ~= "Transition" then
                        if memNum ~= nil and memNum>0 then
                            -- writer:writeln('%s_x = new float[%d];',varName, memNum)
                            -- writer:writeln('%s_y = new float[%d];',varName, memNum)
                            -- if memberType == "GList" or memberType == "GLoader" then
                            --     writer:writeln('%s_width = new float[%d];',varName, memNum)
                            --     writer:writeln('%s_height = new float[%d];',varName, memNum)
                            -- end

                            -- for k=1,#orderItems do
                            --     writer:writeln('%s_x[%d] = %s[%d].x;', varName, k-1, varName, k-1)
                            --     writer:writeln('%s_y[%d] = %s[%d].y;', varName, k-1, varName, k-1)
                            --     if memberType == "GList" or memberType == "GLoader" then
                            --         writer:writeln('%s_width[%d] = %s[%d].width;', varName, k-1, varName, k-1)
                            --         writer:writeln('%s_height[%d] = %s[%d].height;', varName, k-1, varName, k-1)
                            --     end
                            -- end
                        else
                            if memberClass then
                                writer:writeln('%s_x = UIObj.GetChild("%s").x;', varName, memberName)
                                writer:writeln('%s_y = UIObj.GetChild("%s").y;', varName, memberName)
                                if memberType == "GList" or memberType == "GLoader" then
                                    writer:writeln('%s_width = UIObj.GetChild("%s").width;', varName, memberName)
                                    writer:writeln('%s_height = UIObj.GetChild("%s").height;', varName, memberName)
                                end
                            else
                                writer:writeln('%s_x = %s.x;', varName, varName)
                                writer:writeln('%s_y = %s.y;', varName, varName)
                                if memberType == "GList" or memberType == "GLoader" then
                                    writer:writeln('%s_width = %s.width;', varName, varName)
                                    writer:writeln('%s_height = %s.height;', varName, varName)
                                end
                            end
                        end
                    end
                end
                writer:endBlock() --method
            end

            -- writer:writeln()
            -- writer:writeln('protected new GObject GetObj(string name, GComponent parent) => base.GetObj(name, parent);')
            -- writer:writeln()
            -- writer:writeln('protected new Controller GetCtr(string name, GComponent parent) => base.GetCtr(name, parent);')
            -- writer:writeln()
            -- writer:writeln('protected new T Get<T>(string name, GComponent parent) where T : GObject => base.Get<T>(name, parent);')
            -- writer:writeln()
            -- writer:writeln('protected new Transition GetTran(string name, GComponent parent) => base.GetTran(name, parent);')

            writer:endBlock() --class
            writer:endBlock() --namepsace

            writer:save(exportCodePath..'/Gen_'..classInfo.className..'.cs')
        end
    end

    writer:reset()

end

return genCode