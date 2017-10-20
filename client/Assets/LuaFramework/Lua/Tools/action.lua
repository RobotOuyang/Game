require "math"

Action = {};
local this = CtrlManager;

-- power为幂，用于控制减速的幅度
function Action.EaseIn( t, c, power)
	power = power or 3
	return c * math.pow(t, power)
end

-- t1/t2
function Action.EaseOut( t1, t2, c, power)
	power = power or 3
	local t = t1/t2
	return c * (1 - math.pow(1 - t, power))
end

function Action.EaseInOut( t, c, power)
	power = power or 3
	if math.pow(t, 2) < 1 then
		return c / 2 * math.pow(t * 2, power)
	else
		return c / 2 * (2 - math.pow(2 - t * 2, power))
	end
end
