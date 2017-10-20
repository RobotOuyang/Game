module('Battery', package.seeall)

function Bind( coro, obj )
	if not obj then return end
	local time_text = obj.transform:Find("time"):GetComponent("Text")
	local battery_fill = obj.transform:Find("battery/fill"):GetComponent("Image")
	local network_fill = obj.transform:Find("network"):GetComponent("Image")
	coro.start(function ()
		while true do
			if IsNil(time_text) then return end
			battery_fill.fillAmount = BatteryLevel.GetBatteryLevel()/100
			local cur_time = DateTime.Now
			time_text.text = string.format('%02d:%02d', cur_time.Hour, cur_time.Minute)
			local delay = Network.GetDelay()

			network_fill.fillAmount = (2200 - delay) / 2000

			coro.wait(5)
		end
	end)
end