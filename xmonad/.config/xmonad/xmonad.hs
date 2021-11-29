 -- Base
import XMonad
import System.Exit
import qualified XMonad.StackSet as W

 -- Data
import Data.Monoid
import qualified Data.Map as M

 -- Layouts
import XMonad.Layout.ResizableTile

 -- Layout Modifiers
import XMonad.Layout.LayoutModifier
import XMonad.Layout.Renamed
import XMonad.Layout.Spacing
import XMonad.Layout.NoBorders

 -- Util
import XMonad.Util.Run
import XMonad.Util.SpawnOnce

 -- Hooks
import XMonad.Hooks.ManageDocks
import XMonad.Hooks.SetWMName
import XMonad.Config.Dmwit (altMask)
import XMonad.Hooks.DynamicLog (dynamicLogWithPP, xmobarPP, PP (ppOutput, ppCurrent, ppVisible, ppHidden, ppHiddenNoWindows, ppTitle, ppSep, ppUrgent, ppOrder), wrap, shorten, xmobarColor)
import XMonad.Util.EZConfig (additionalKeysP)
import XMonad.Hooks.EwmhDesktops (ewmh)
import Data.Strict.Maybe (fromJust)
import XMonad.Layout.ShowWName

myTerminal :: String
myTerminal = "alacritty"

myModMask :: KeyMask
myModMask = mod4Mask

myBorderWidth = 2

myFocusFollowsMouse :: Bool
myFocusFollowsMouse = True

myClickJustFocuses :: Bool
myClickJustFocuses = False

-- The default number of workspaces (virtual screens) and their names.
-- By default we use numeric strings, but any string may be used as a
-- workspace name. The number of workspaces is determined by the length
-- of this list.
--
-- A tagging example:
--
-- > workspaces = ["web", "irc", "code" ] ++ map show [4..9]
--

-- Border colors for unfocused and focused windows, respectively.
--
myNormalBorderColor = "#dddddd"
myFocusedBorderColor = "#ff0000"

myWorkspaces = ["1","2","3","4","5","6","7","8","9"]
myWorkspaceIndicies = M.fromList $ zip myWorkspaces [1..]

-- clickable ws = "<action=xdotool key super+"++show i++">"++ws++"</action>"
--  where i = fromJust $ M.lookup ws myWorkspaceIndicies

------------------------------------------------------------------------
-- Key bindings. Add, modify or remove key bindings here.
--
myKeys :: [(String, X ())]
myKeys =

    -- launch a terminal
    [ ("M-<Return>", spawn myTerminal)

    -- launch dmenu
    , ("M-<Space>", spawn "dmenu_run -fn 'SourceCodePro Nerd Font-10'")

    -- launch gmrun
    , ("M-S-p", spawn "gmrun")

    -- close focused window
    , ("M-d", kill)

     -- Rotate through the available layout algorithms
    , ("M-M1-<Space>", sendMessage NextLayout)

    --  Reset the layouts on the current workspace to default
--    , ("M-M1-S-<Space>", setLayout $ XMonad.layoutHook conf)

    -- Resize viewed windows to the correct size
    , ("M-n", refresh)

    -- Move focus to the next window
    , ("M-<Tab>", windows W.focusDown)

    -- Move focus to the next window
    , ("M-j", windows W.focusDown)

    -- Move focus to the previous window
    , ("M-k", windows W.focusUp)

    -- Move focus to the master window
    , ("M-m", windows W.focusMaster)

    -- Swap the focused window and the master window
    , ("M-S-<Return>", windows W.swapMaster)

    -- Swap the focused window with the next window
    , ("M-S-j", windows W.swapDown)

    -- Swap the focused window with the previous window
    , ("M-S-k", windows W.swapUp)

    -- Shrink the master area
    , ("M-h", sendMessage Shrink)

    -- Expand the master area
    , ("M-l", sendMessage Expand)

    -- Push window back into tiling
    , ("M-t", withFocused $ windows . W.sink)

    -- Increment the number of windows in the master area
    , ("M-,", sendMessage (IncMasterN 1))

    -- Deincrement the number of windows in the master area
    , ("M-.", sendMessage (IncMasterN (-1)))

    -- Toggle the status bar gap
    -- Use this binding with avoidStruts from Hooks.ManageDocks.
    -- See also the statusBar function from Hooks.DynamicLog.
    --
    -- , ((modm              , b     ), sendMessage ToggleStruts)

    -- Quit xmonad
    , ("M-S-q", io exitSuccess)

    -- Restart xmonad
    , ("M-q", spawn "xmonad --recompile; killall xmobar; xmonad --restart")

    -- Run xmessage with a summary of the default keybindings (useful for beginners)
    , ("M-S-/", spawn ("echo \"" ++ help ++ "\" | xmessage -file -"))
    ]
    -- ++

    --
    -- mod-[1..9], Switch to workspace N
    -- mod-shift-[1..9], Move client to workspace N
    --
    --[("m-M-k", windows $ f i)
    --    | (i, k) <- zip (XMonad.workspaces conf) [1 .. 9]
    --    , (f, m) <- [(W.greedyView, 0), (W.shift, shiftMask)]]
    -- ++

    --
    -- mod-{w,e,r}, Switch to physical/Xinerama screens 1, 2, or 3
    -- mod-shift-{w,e,r}, Move client to screen 1, 2, or 3
    --
    --[((m .|. modm, key), screenWorkspace sc >>= flip whenJust (windows . f))
    --    | (key, sc) <- zip [w, e, r] [0..]
    --    , (f, m) <- [(W.view, 0), (W.shift, shiftMask)]]


------------------------------------------------------------------------
-- Mouse bindings: default actions bound to mouse events
--
myMouseBindings (XConfig {XMonad.modMask = modm}) = M.fromList

    -- mod-button1, Set the window to floating mode and move by dragging
    [ ((modm, button1), \w -> focus w >> mouseMoveWindow w
                                       >> windows W.shiftMaster)

    -- mod-button2, Raise the window to the top of the stack
    , ((modm, button2), \w -> focus w >> windows W.shiftMaster)

    -- mod-button3, Set the window to floating mode and resize by dragging
    , ((modm, button3), \w -> focus w >> mouseResizeWindow w
                                       >> windows W.shiftMaster)

    -- you may also bind events to the mouse scroll wheel (button4 and button5)
    ]

------------------------------------------------------------------------
-- Layouts:

-- You can specify and transform your layouts by modifying these values.
-- If you change layout bindings be sure to use 'mod-shift-space' after
-- restarting (with 'mod-q') to reset your layout state to the new
-- defaults, as xmonad preserves your old layout settings by default.
--
-- The available layouts.  Note that each layout is separated by |||,
-- which denotes layout choice.
--

--Makes setting the spacingRaw simpler to write. The spacingRaw module adds a configurable amount of space around windows.
mySpacing :: Integer -> l a -> XMonad.Layout.LayoutModifier.ModifiedLayout Spacing l a
mySpacing i = spacingRaw False (Border i i i i) True (Border i i i i) True

-- Below is a variation of the above except no borders are applied
-- if fewer than two windows. So a single window has no gaps.
mySpacing' :: Integer -> l a -> XMonad.Layout.LayoutModifier.ModifiedLayout Spacing l a
mySpacing' i = spacingRaw True (Border i i i i) True (Border i i i i) True

tall = renamed [Replace "tall"]
       $ smartBorders
       $ mySpacing 8
       $ ResizableTall 1 (3/100) (1/2) []

myLayoutHook = avoidStruts myDefaultLayout
  where
    myDefaultLayout = withBorder myBorderWidth tall


myShowWNameTheme :: SWNConfig
myShowWNameTheme = def
  {
    swn_font = "xft:SourceCodePro:bold:size=60",
    swn_fade = 1.0,
    swn_bgcolor = "#1d2021",
    swn_color = "#ebdbb2"
  }

------------------------------------------------------------------------
-- Window rules:

-- Execute arbitrary actions and WindowSet manipulations when managing
-- a new window. You can use this to, for example, always float a
-- particular program, or have a client always appear on a particular
-- workspace.
--
-- To find the property name associated with a program, use
-- > xprop | grep WM_CLASS
-- and click on the client you're interested in.
--
-- To match on the WM_NAME, you can use 'title' in the same way that
-- 'className' and 'resource' are used below.
--
myManageHook = composeAll
    [ className =? "MPlayer"        --> doFloat
    , className =? "Gimp"           --> doFloat
    , resource  =? "desktop_window" --> doIgnore
    , resource  =? "kdesktop"       --> doIgnore ]

------------------------------------------------------------------------
-- Event handling

-- * EwmhDesktops users should change this to ewmhDesktopsEventHook
--
-- Defines a custom handler function for X Events. The function should
-- return (All True) if the default handler is to be run afterwards. To
-- combine event hooks use mappend or mconcat from Data.Monoid.
--

------------------------------------------------------------------------
-- Status bars and logging

-- Perform an arbitrary action on each internal state change or X event.
-- See the 'XMonad.Hooks.DynamicLog' extension for examples.
--

------------------------------------------------------------------------
-- Startup hook

-- Perform an arbitrary action each time xmonad starts or is restarted
-- with mod-q.  Used by, e.g., XMonad.Layout.PerWorkspace to initialize
-- per-workspace layout choices.
--
-- By default, do nothing.
myStartupHook :: X ()
myStartupHook = do
    spawnOnce "picom &"
    spawnOnce "feh --bg-scale ~/Pictures/wallpapers/landscape-minimal-panaromic.jpg ~/Pictures/wallpapers/west-of-the-sun.jpg"
    setWMName "LG3D"

------------------------------------------------------------------------
-- Now run xmonad with all the defaults we set up.

-- Run xmonad with the settings you specify. No need to modify this.
--
main = do
  xmproc0 <- spawnPipe "xmobar -x 0 $HOME/.config/xmobar/xmobarrc"
  xmproc1 <- spawnPipe "xmobar -x 1 $HOME/.config/xmobar/xmobarrc"
  xmonad $ ewmh def
    {
      -- simple stuff
        terminal           = myTerminal,
        focusFollowsMouse  = myFocusFollowsMouse,
        clickJustFocuses   = myClickJustFocuses,
        borderWidth        = myBorderWidth,
        modMask            = myModMask,
        workspaces         = myWorkspaces,
        normalBorderColor  = myNormalBorderColor,
        focusedBorderColor = myFocusedBorderColor,

      -- key bindings
        mouseBindings      = myMouseBindings,

      -- hooks, layouts
        startupHook        = myStartupHook,
        layoutHook         = showWName' myShowWNameTheme myLayoutHook,
        manageHook         = myManageHook <+> manageDocks,
        handleEventHook    = docksEventHook,
        logHook            = dynamicLogWithPP $ xmobarPP
          {
            ppOutput = \x -> hPutStrLn xmproc0 x
                          >> hPutStrLn xmproc1 x,
            ppCurrent = xmobarColor "#d3869b" "" . wrap "<box type=Bottom width=2 mb=2 color=#d3869b>" "</box>",
            ppVisible = xmobarColor "#d3869b" "", -- . clickable,
            ppHidden = xmobarColor "#83a598" "" . wrap "<box type=Top width=2 mt=2 color=#83a598>" "</box>", -- . clickable,
            ppHiddenNoWindows = xmobarColor "#83a598" "", -- . clickable,
            ppTitle = xmobarColor "#83a598" "" . shorten 60,
            ppSep = "<fc=#a89984> <fn=0>|</fn> </fc>",
            ppUrgent = xmobarColor "#fe8019" "" . wrap "!" "!",
            ppOrder = \(ws:l:t:ex) -> [ws,l]++ex++[t]
          }
    } `additionalKeysP` myKeys

-- | Finally, a copy of the default bindings in simple textual tabular format.
help :: String
help = unlines ["The default modifier key is 'alt'. Default keybindings:",
    "",
    "-- launching and killing programs",
    "mod-Shift-Enter  Launch xterminal",
    "mod-p            Launch dmenu",
    "mod-Shift-p      Launch gmrun",
    "mod-Shift-c      Close/kill the focused window",
    "mod-Space        Rotate through the available layout algorithms",
    "mod-Shift-Space  Reset the layouts on the current workSpace to default",
    "mod-n            Resize/refresh viewed windows to the correct size",
    "",
    "-- move focus up or down the window stack",
    "mod-Tab        Move focus to the next window",
    "mod-Shift-Tab  Move focus to the previous window",
    "mod-j          Move focus to the next window",
    "mod-k          Move focus to the previous window",
    "mod-m          Move focus to the master window",
    "",
    "-- modifying the window order",
    "mod-Return   Swap the focused window and the master window",
    "mod-Shift-j  Swap the focused window with the next window",
    "mod-Shift-k  Swap the focused window with the previous window",
    "",
    "-- resizing the master/slave ratio",
    "mod-h  Shrink the master area",
    "mod-l  Expand the master area",
    "",
    "-- floating layer support",
    "mod-t  Push window back into tiling; unfloat and re-tile it",
    "",
    "-- increase or decrease number of windows in the master area",
    "mod-comma  (mod-,)   Increment the number of windows in the master area",
    "mod-period (mod-.)   Deincrement the number of windows in the master area",
    "",
    "-- quit, or restart",
    "mod-Shift-q  Quit xmonad",
    "mod-q        Restart xmonad",
    "mod-[1..9]   Switch to workSpace N",
    "",
    "-- Workspaces & screens",
    "mod-Shift-[1..9]   Move client to workspace N",
    "mod-{w,e,r}        Switch to physical/Xinerama screens 1, 2, or 3",
    "mod-Shift-{w,e,r}  Move client to screen 1, 2, or 3",
    "",
    "-- Mouse bindings: default actions bound to mouse events",
    "mod-button1  Set the window to floating mode and move by dragging",
    "mod-button2  Raise the window to the top of the stack",
    "mod-button3  Set the window to floating mode and resize by dragging"]
