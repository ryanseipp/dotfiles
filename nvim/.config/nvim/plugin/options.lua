local opt = vim.opt

-- :help <opt>
opt.cmdheight = 1
opt.showmatch = true
opt.relativenumber = true
opt.number = true
opt.ignorecase = true
opt.smartcase = true
opt.hidden = true
opt.cursorline = true
opt.equalalways = true
opt.splitright = true
opt.splitbelow = true
opt.updatetime = 1000
opt.scrolloff = 10

opt.autoindent = true
opt.cindent = true
opt.wrap = true

opt.tabstop = 4
opt.shiftwidth = 4
opt.softtabstop = 4
opt.expandtab = true

opt.breakindent = true
opt.linebreak = true

opt.foldmethod = "marker"
opt.foldlevel = 0
opt.modelines = 1

opt.belloff = "all"

opt.clipboard = "unnamedplus"

opt.inccommand = "split"
opt.shada = { "!", "'1000", "<50", "s10", "h" }

opt.mouse = "n"

opt.formatoptions = opt.formatoptions
  - "a"
  - "t"
  + "c"
  + "q"
  - "o"
  + "r"
  + "n"
  + "j"
  - "2"
