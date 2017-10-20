
require "Common/TipText"

SpecialUiController = {}
local this = SpecialUiController

function this.Init()
	-- body
	resMgr:LoadPrefab('message',  { 'text_notice'}, function (objs)
		this.text_notice = objs[0]
	 end)

	resMgr:LoadTexture('common',  { 'default_cat', 'default_male', 'default_female', 'empty_avatar' }, function (objs)
		this.avatar_tex = {}
		this.avatar_tex.default = objs[0]:GetComponent('Image')
		this.avatar_tex.male = objs[1]:GetComponent('Image')
		this.avatar_tex.female = objs[2]:GetComponent('Image')
		this.avatar_tex.empty = objs[3]:GetComponent('Image')
	 end)
end

function this.ShowTextMessage(parent, text, gold_table, delay_time)
	-- body
	local canvas = GameObject.FindGameObjectWithTag("GuiCanvas")
	local parent = parent or canvas.transform
	local text_obj = panelMgr:CreateEffect(parent, this.text_notice, true)
	local position = parent or parent.position and Vector3.New(0, 0, 0)
	gold_table = gold_table or {}
	for k, v in ipairs(gold_table) do
		gold_table[k] = "<color=fae003>" .. v .. "/color"
	end
	local text = string.format(text, unpack(gold_table))
	text_obj.transform:Find('content'):GetComponent('Text').text = text
	if delay_time then
		AnimationManager.PlayDelayFade(text_obj, delay_time)
	else
		AnimationManager.PlayTextNoticeAnimation(text_obj)
	end
end

function this.ShowTextMessageSpadeAce( parent, text, frames, gold_table )
	local text_obj = panelMgr:CreateEffect(parent, this.text_notice, true)
	local position = parent or parent.position and Vector3.New(0, 0, 0)
	gold_table = gold_table or {}
	for k, v in ipairs(gold_table) do
		gold_table[k] = "<color=fae003>" .. v .. "/color"
	end
	local text = string.format(text, unpack(gold_table))
	text_obj.transform:Find('content'):GetComponent('Text').text = text
	AnimationManager.PlayTextNoticeAnimation(text_obj, frames)
end

function this.ShowTextMessageWithCode( parent, module_name, code )
	local module_tips = TipText.Code2Tips[module_name]
	local error_message
	if module_tips then
		error_message = module_tips[code]
		if not error_message then error_message = module_tips["default"] end
	end
	if not error_message then error_message = TipText.Code2Tips["default"] end
	this.ShowTextMessage(parent, error_message)
end

function this.GetAvatarSprite( sex )
	-- body
	local sprite
	if sex == 0 then
		sprite = this.avatar_tex.empty.sprite
	elseif sex == 1 then 
		sprite = this.avatar_tex.male.sprite
	elseif sex == 2 then
		sprite = this.avatar_tex.female.sprite
	else
		sprite = this.avatar_tex.default.sprite
	end
	return sprite
end

function this.ShowLoading( text, time, out_time_func )
	-- body	
	CtrlManager.Open('WaitingNetworkCtrl', {text=text, time=time, func = out_time_func})
end

function this.HideLoading( )
	-- body
	CtrlManager.Close('WaitingNetworkCtrl')
end