-- color scheme
vim.o.termguicolors = true
vim.o.background = "dark"
vim.cmd [[colorscheme gruvbox-flat]]
vim.g.gruvbox_flat_style = "hard"

-- status line
require('lualine').setup {
  options = {
    theme = 'gruvbox-flat'
  }
}
