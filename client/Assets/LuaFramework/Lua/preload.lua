PreLoad = {}
local this = PreLoad

local preload_list = 
{
	{load_type = 'prefab', files = {'gamehall/shop', {'ShopPanel'}}},
	{load_type = "prefab", files = {"slots/items", {"icon1", "icon2", "icon3", "icon4", "icon5", "icon6", "icon7", "icon8", "icon9", "icon10",
										"1", "2", "3", "4", "5", "6", "7", "8", "9", "blur_icon1", "blur_icon2", "blur_icon3", "blur_icon4",
										"blur_icon5", "blur_icon6", "blur_icon7", "blur_icon8", "blur_icon9", "his_item"}}},
	{load_type = "prefab", files = {'lhdbeffect', {'slots_wined_coin', 'coin_gain', "egg", "beer", "water", "brick", "knife", "kiss", "bomb", "flower"}}},
	{load_type = "texture", files = {"slots/icons", {"1", "2", "3", "4", "5", "6", "7", "8", "9"}}},
	{load_type = "texture", files = {"slots/game_compare", {"dice1", "dice2", "dice3", "dice4", "dice5", "dice6", "his_big", "his_equal", "his_small"}}},
	{load_type = "texture", files = {"slots/common", {"green_hollow", "purple_hollow", "red_hollow", "yellow_hollow", "red", "green", "purple", "yellow"}}},
	{load_type = "prefab", files = {"Slots/model", {"long", "zhongyitang", "titianxingdao", "songjiang", "linchong", "luzhishen"}}},
	{load_type = "prefab", files = {"Slots/prefabs", {"songjiang_effect", "linchong_effect", "luzhishen_effect"}}},
	{load_type = "prefab", files = {'chat', {"no_seat_sentense_item"}}},
	{load_type = "prefab", files = {'MassiveBattle/items_prefebs', {'pop_up_chat'}}},
	{load_type = "texture", files = {'massive_battle/cards', { 'card_black_2', 'card_black_3', 'card_black_4', 'card_black_5', 'card_black_6', 'card_black_7',
		'card_black_8', 'card_black_9', 'card_black_10', 'card_black_j', 'card_black_q', 'card_black_k', 'card_black_a', 'card_red_2',
		'card_red_3', 'card_red_4', 'card_red_5', 'card_red_6', 'card_red_7', 'card_red_8', 'card_red_9', 'card_red_10', 'card_red_j',
		'card_red_q', 'card_red_k', 'card_red_a', 
		'card_black', 'card_heart', 'card_blossom', 'card_block', 'card_big_black', 'card_big_heart', 'card_big_blossom', 'card_big_block',
		'card_type_1', 'card_type_4', 'card_type_7', 'card_type_5', 'card_type_6', 'card_type_3', 'card_type_2',
		"color_black", "color_black_bottom", 'color_heart', 'color_heart_bottom',
		'color_blossom', 'color_blossom_bottom', "color_block", "color_block_bottom"}}},
	{load_type = "prefab", files = {'gamehall/items', {'slots_small_game_list'}}},
	{load_type = "prefab", files = {'GoldenShark/item_prefabs', {'bullion', 'brick', 'small_gold'}}},
	{load_type = "prefab", files = {'LhdbMain/items', {"area_seat_item", "select_area_item", "lhdb_seat"}}},
	{load_type = "texture", files = {'numbers', {"lhdb_seat_0", "lhdb_seat_1", "lhdb_seat_2", "lhdb_seat_3", "lhdb_seat_4",
 							"lhdb_seat_5", "lhdb_seat_6", "lhdb_seat_7", "lhdb_seat_8", "lhdb_seat_9"}}},
 	{load_type = "prefab", files = {'gems', {'drill', 
	'gem_1_1', 'gem_1_2', 'gem_1_3', 'gem_1_4', 'gem_1_5',
	'gem_2_1', 'gem_2_2', 'gem_2_3', 'gem_2_4', 'gem_2_5', 
	'gem_3_1', 'gem_3_2', 'gem_3_3', 'gem_3_4', 'gem_3_5', 
	'broken_star', 'dice1', 'dice2', 'dice3', 'dice4'}}},
	{load_type = "prefab", files = {'lhdbeffect', {'gem_gen', 'coin_gain', 'wined_coin', 'stage_1', 'stage_2', 'stage_3',
		'combo_1', 'combo_2', 'combo_3'}}},
	{load_type = "texture", files = {'gems/lvl1', {'1002_0000', '1003_0000', '1004_0000', '1005_0000', '1006_0000'}}},
	{load_type = "texture", files = {'gems/lvl2', {'1007_0000', '1008_0000', '1009_0000', '1010_0000', '1011_0000'}}},
	{load_type = "texture", files = {'gems/lvl3', {'1012_0000', '1013_0000', '1014_0000', '1015_0000', '1016_0000'}}},
	{load_type = "prefab", files = {'MassiveBattle/items_prefebs', {'bullion', 'brick', 'small_gold', 'card'}}},
	{load_type = "prefab", files = {'chat', {'talk_item', 'info_item', 'bubble_item'}}},
	{load_type = "prefab", files = {'LhdbEffect', {"egg", "beer", "water", "brick", "knife", "kiss", "bomb", "flower"}}},
	{load_type = "prefab", files = {'chat', {"left_sentense_item", "right_sentense_item", "down_left_sentense_item", "no_seat_sentense_item"}}},
	{load_type = "prefab", files = {'MassiveBattle/items_prefebs', {'bullion', 'brick', 'small_gold', 'card'}}},
	{load_type = "prefab", files = {'MassiveBattle/items_prefebs/active_items', {'active_panel', 'player', "bet_button", "button_repeat", 'bet_area', 'ante_area', }}},
	{load_type = "prefab", files = {'ZJH/items', {'load', 'card', 'shandian_card', 'shandian_head'}}},
	{load_type = "prefab", files = {'ZJH/items/golds', {}}},
	{load_type = "prefab", files = {'lhdbeffect', {'coin_gain'}}},
	{load_type = "texture", files = {'zjh/main/btn_color', {}}},
	{load_type = "texture", files = {'zjh/status', {'chat_bps', 'chat_qp'}}},
	{load_type = "prefab", files = {'ZJH/items/status', {'black_left', 'black_right', 'look_left', 'look_right'}}},
}

if LoadingCtrl.avoid_first_patch == "no" then
	table.insert(preload_list, 1, {load_type = 'prefab', files = {'gamehall/items', {"notice", "email", "vip_mall", "money_tree", "free_coin"}}})
	table.insert(preload_list, 1, {load_type = 'prefab', files = {'gamehall/items', {'pop_up_chat'}}})
	table.insert(preload_list, 1, {load_type = 'prefab', files = {'gamehall/items', {"lhdb", "massive_battle", "golden_shark", "zjh", "shui_hu_zhuan"}}})
end

local finish_list = {}
local is_loading = false

function this.check_next(waits)
	if waits > 0 then coroutine.step(waits) end
	if not is_loading then return end
	for k,v in ipairs(preload_list) do
		if not finish_list[k] then
			this.load_index(k)
			return
		end
	end
end

function this.load_index( index )
	local v = preload_list[index]
	-- log('<color=red>preloading index : </color>' .. index)
	finish_list[index] = true
	if v.load_type == "texture" then
		resMgr:LoadTexture(v.files[1], v.files[2], function(objs)		
			coroutine.start(this.check_next, 1)
		end)
	else
		resMgr:LoadPrefab(v.files[1], v.files[2], function(objs)
			coroutine.start(this.check_next, 1)
		end)
	end
end

function this.Start()
	-- body
	is_loading = true
	coroutine.start(this.check_next, 0)
end

function this.Stop()
	is_loading = false
end

