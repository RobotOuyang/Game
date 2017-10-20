
SoundManager = {};
local this = SoundManager;

-- 临时音效，背景音乐直接在场景摄像机里，不在此列
local all_audio_clips = {}		-- clip一般用于给加了audiosource的单位指定音效
local audio_prefabs = {}	-- 这个预制体是自动生成的，用来播放一次性音效
local audio_object = {}	-- 缓存音效物件，不频繁创建销毁

function SoundManager.Init( )
	resMgr:LoadAudio("", {'button.mp3'}, this.OnAudioInit)
	resMgr:LoadPrefab("audioprefab", {}, this.OnPrefabInit)

	this.conf = FILE_DB.LoadFileDB('__setting__')

	if this.conf.tog_music_effect == nil then this.conf.tog_music_effect = true end
	if this.conf.tog_music == nil then this.conf.tog_music = true end

	-- 定时把不用的音效disable
	coroutine.start(function ( ...)
		while true do
			for name, objs in pairs(audio_object) do
				for k,v in ipairs(objs) do
					if v ~= nil and not v.isPlaying then
						v.gameObject:SetActive(false)
					end
				end
			end
			coroutine.wait(1)
		end
	end)
end

function SoundManager.PlaySoundOnce( name )
	if not this.conf.tog_music_effect then return end
	local audio_played = false

	-- 考虑到这个音效可能在被其他的播，就创建个新的
	if audio_object[name] ~= nil then
		for k,v in ipairs(audio_object[name]) do
			if v ~= nil and not v.isPlaying then
				v.gameObject:SetActive(true)
				v.loop = false
				v:Play()
				audio_played = true
				break
			end
		end
		-- 保证音效对象池的某个对象不会无限制增长
		if #audio_object[name] > 15 then
			return
		end
	end

	if not audio_played and audio_prefabs[name] ~= nil then
		-- 创建后自动会play
		local new_one = newObject(audio_prefabs[name]):GetComponent('AudioSource')
		if audio_object[name] == nil then
			audio_object[name] = { new_one }
		else
			table.insert(audio_object[name], new_one)
		end
	end
end

function SoundManager.StopSound( name )
	-- 考虑到这个音效可能在被其他的播，就创建个新的
	if audio_object[name] ~= nil then
		for k,v in ipairs(audio_object[name]) do
			if v ~= nil and v.isPlaying then
				v.loop = false
				v:Stop()
			end
		end
	end
end

-- 
function SoundManager.PlaySoundLoop( name )
	if not this.conf.tog_music_effect then return end	-- music off

	-- 考虑到这个音效可能在被其他的播，就创建个新的
	if audio_object[name] ~= nil then
		for k,v in ipairs(audio_object[name]) do
			if v ~= nil and not v.isPlaying then
				v.gameObject:SetActive(true)
				v.loop = true
				v:Play()
				return v
			end
		end
	end

	if audio_prefabs[name] ~= nil then
		-- 创建后自动会play
		local new_one = newObject(audio_prefabs[name]):GetComponent('AudioSource')
		new_one.loop = true
		if audio_object[name] == nil then
			audio_object[name] = { new_one }
		else
			table.insert(audio_object[name], new_one)
		end
		return new_one
	end
end

-- 找当前场景的GUICamera，这个在任何panel被创建时会被引擎调用
function SoundManager.BackgroundMusicCheck()
	panelMgr:BackgroundMusicCheck(this.conf.tog_music)
end

function SoundManager.OnPrefabInit( objs )
	-- body
	for i = 0, objs.Length - 1 do
		audio_prefabs[objs[i].name] = objs[i]
	end
end

-- 增加一个音效路径，有一些音效可能是分包加载，特定界面才需要加载的，根路径为prefab。
-- 注意：音效名要全局唯一，否则可能会覆盖
function SoundManager.AddAudioPath( path )
	resMgr:LoadPrefab(path, {}, function ( objs )
		for i = 0, objs.Length - 1 do
			audio_prefabs[objs[i].name] = objs[i]
		end
	end)
end

function SoundManager.OnAudioInit( objs )
	--
	all_audio_clips.button = objs[0]
end

-- 凡是添加了响应的按钮都会播声音
function SoundManager.PlayButtonSound( btn, audio )
	-- body
	if not this.conf.tog_music_effect then return end
	if not audio then return end

	if audio.clip == nil then
		audio.clip = all_audio_clips.button
	end
	audio:Play()
	-- this.PlaySoundOnce('gain_coin')
end

function SoundManager.PlayToggleSound( tog, audio )
	-- body
	this.PlayButtonSound(nil, audio)
end

function this.ClearCache()
	-- body
	audio_object = {}
end