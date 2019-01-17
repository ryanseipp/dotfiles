#!/usr/bin/env sh

killall -q polybar

# is your bar running? better go catch it
polybar_proc=$(pgrep -u $UID -x polybar)

for i in ${polybar_proc}; do
  kill -9 $i
done

while pgrep -U $UID -x polybar > /dev/null; do sleep 1; done

# launch bar(s)
polybar top &

echo "Bar(s) launched..."
