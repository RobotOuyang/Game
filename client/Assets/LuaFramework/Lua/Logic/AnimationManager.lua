AnimationManager = {}
local this = AnimationManager

local function up_and_fade( obj, up_frames, up_speed, fade_frames )
	local rect = obj:GetComponent('RectTransform')
	local canvas_group = obj:GetComponent('CanvasGroup')

	canvas_group.alpha = 1
	for i = 1, up_frames do
		if IsNil(rect) or IsNil(canvas_group) then return end
		local position = rect.anchoredPosition
		position.y = position.y + up_speed
		rect.anchoredPosition = position

		if i > up_frames - fade_frames then
			canvas_group.alpha = canvas_group.alpha - 1 / fade_frames
		end
		coroutine.step()
	end
	panelMgr:CollectEffect(obj)
end

local function fade( obj, fade_frames )
	local canvas_group = obj:GetComponent('CanvasGroup')
	for i = 1, fade_frames do
		if IsNil(canvas_group) then return end
		canvas_group.alpha = 1 - i / fade_frames
		coroutine.step()
	end
	if IsNil(obj) then return end
	panelMgr:CollectEffect(obj)
end

local function stay_and_fade( obj, wait_time, fade_time )
	-- body
	fade_time = fade_time or 70
	local canvas_group = obj:GetComponent('CanvasGroup')
	canvas_group.alpha = 1
	
	coroutine.wait(wait_time)

	for i = 1, 10 do
		if IsNil(canvas_group) then return end

		canvas_group.alpha = canvas_group.alpha - 0.1
		coroutine.wait(fade_time / 10)
	end
	panelMgr:CollectEffect(obj)
end

local function scale_big_and_disapear( obj , scale )
	local rect = obj:GetComponent('RectTransform')
	rect.localScale = Vector3.New()
	local scale_bigger = scale / 8
	local scale_smaller = (scale - 1) / 5 

	for i = 1, 20 do
		if IsNil(rect) then return end

		local scale = rect.localScale

		if i > 15 then
			scale.x = scale.x - 0.04
			scale.y = scale.y - 0.04
			rect.localScale = scale
		elseif i <= 8 then
			scale.x = scale.x + 0.15
			scale.y = scale.y + 0.15
			rect.localScale = scale
		end

		coroutine.step()
	end
	panelMgr:CollectEffect(obj)
end

function this.PlayGemObtainAnimation( obj )
	coroutine.start(up_and_fade, obj, 20, 5, 10)
end

function this.PlayTextNoticeAnimation( obj )
	coroutine.start(fade, obj, 70)
end

function this.PlayTextNoticeAnimationExt( obj, frames )
	coroutine.start(fade, obj, frames)
end

function this.PlayComboAnimation( obj, count )
	local scale
	if count <=4 then
		scale = 1.2
	elseif count <= 6 then
		scale = 1.3
	else
		scale = 1.4
	end
	coroutine.start(scale_big_and_disapear, obj, scale)
end

function this.PlayDelayFade( obj, delay, fade )
	-- body
	coroutine.start(stay_and_fade, obj, delay, fade)
end

local function move_with_speed_and_acc( obj, speed, to_pos, acc, frames, wait)
	-- 先炸开
	while wait > 0 do
		wait = wait - 1
		coroutine.step()
	end

	obj:SetActive(true)
	while frames > 0 do
		if IsNil(obj) then return end

		local position = obj.transform.position
		position.x = position.x + speed.x
		position.y = position.y + speed.y

		speed.x = speed.x * 1
		speed.y = speed.y * 1

		obj.transform.position = position
		coroutine.step()
		frames = frames - 1
	end

	local collect_speed = 0.1
	while VectorFunc.Distance(obj.transform.position, to_pos) > 1 do
		if IsNil(obj) then return end
		local position = obj.transform.position
		obj.transform.position = Vector3.MoveTowards(position, to_pos, collect_speed) 
		collect_speed = collect_speed + acc
		coroutine.step()
	end
	panelMgr:CollectEffect(obj)
end

function this.PlayCoinAnimation( objs, to_pos )
	-- body
	local acc = 2;
	local rate = 15 / #objs
	-- 最少15个
	local speed_rate = 1 + (#objs - 15) / 100
	local wait = 1
	local animation_count = 0

	for k,v in ipairs(objs) do
		v:SetActive(false)

		-- 当喷的金币数量变多时，间隔会变短，范围会变大
		local speed = VectorFunc.GetRandomVector2(math.random(2, 4)) * speed_rate
		coroutine.start(move_with_speed_and_acc, v, speed, to_pos, 1, 10, wait)
		wait = wait + 1 / speed_rate
	end

end

function this.PlayEmotion(obj, from_pos, to_pos, move_data, begin_frame_count, end_frame_count, scale)
	obj:GetComponent("AniEventHandler"):RegCallback(function(event_type)
		coroutine.start(function()
			end_frame_count = end_frame_count or 3
			while end_frame_count > 0 do
				end_frame_count = end_frame_count - 1
				coroutine.step()
			end
			panelMgr:CollectEffect(obj)
		end)
		
	end)
	coroutine.start(function()
		obj:SetActive(true)
		obj.transform.position = from_pos
		if scale then
			local rect = obj:GetComponent('RectTransform')
			local rectScale = rect.localScale
			rectScale.x = rectScale.x * scale
			-- rectScale.y = rectScale.y * scale
			rect.localScale = rectScale
		end
		begin_frame_count = begin_frame_count or 3
		while begin_frame_count > 0 do
			begin_frame_count = begin_frame_count - 1
			coroutine.step()
		end
		obj:GetComponent("Animator"):SetBool("begin_to_move", true)
	
		local move_speed = VectorFunc.Distance(obj.transform.position, to_pos)*move_data.sample/move_data.frame_count/60

		while VectorFunc.Distance(obj.transform.position, to_pos) > 1 do
			if IsNil(obj) then return end
			local position = obj.transform.position
			obj.transform.position = Vector3.MoveTowards(position, to_pos, move_speed)
			-- coroutine.wait(0.1)
			coroutine.step()
		end

		obj.transform.position = to_pos
		obj:GetComponent("Animator"):SetBool("move_to_play", true)
	end)
end

function this.PlayBeerEmotionOneSide(obj, from_pos, to_pos, move_data, begin_frame_count, end_frame_count, scale)
	obj:GetComponent("AniEventHandler"):RegCallback(function(event_type)
		coroutine.start(function()
			end_frame_count = end_frame_count or 3
			while end_frame_count > 0 do
				end_frame_count = end_frame_count - 1
				coroutine.step()
			end

			panelMgr:CollectEffect(obj)
		end)
	end)
	coroutine.start(function()
		obj:SetActive(true)
		obj.transform.position = from_pos
		if scale then
			local rect = obj:GetComponent('RectTransform')
			local rectScale = rect.localScale
			rectScale.x = rectScale.x * scale
			rect.localScale = rectScale
		end
		begin_frame_count = begin_frame_count or 3
		while begin_frame_count > 0 do
			begin_frame_count = begin_frame_count - 1
			coroutine.step()
		end
		
		local move_speed = VectorFunc.Distance(obj.transform.position, to_pos)*move_data.sample/move_data.frame_count/20

		obj:GetComponent("Animator"):SetBool("begin_to_approach", true)
		while VectorFunc.Distance(obj.transform.position, to_pos) > 1 do
			if IsNil(obj) then return end
			local position = obj.transform.position
			obj.transform.position = Vector3.MoveTowards(position, to_pos, move_speed)
			-- coroutine.wait(0.1)
			coroutine.step()
		end

		coroutine.wait(0.3)

		obj:GetComponent("Animator"):SetBool("approach_to_detach", true)
		while VectorFunc.Distance(obj.transform.position, from_pos) > 1 do
			if IsNil(obj) then return end
			local position = obj.transform.position
			obj.transform.position = Vector3.MoveTowards(position, from_pos, move_speed)
			-- coroutine.wait(0.05)
			coroutine.step()
		end
	end)
end

function this.PlayBeerEmotion(obj1, obj2, from_pos, to_pos, move_data, begin_frame_count, end_frame_count, scale)
	local middle_pos = Vector3.New((from_pos.x + to_pos.x)/2, (from_pos.y + to_pos.y)/2, (from_pos.z + to_pos.z)/2)
	this.PlayBeerEmotionOneSide(obj1, from_pos, middle_pos, move_data, begin_frame_count, end_frame_count, 1)
	this.PlayBeerEmotionOneSide(obj2, to_pos, middle_pos, move_data, begin_frame_count, end_frame_count, -1)
end
