-- color scheme
if vim.fn.has('termguicolors') then
  vim.o.termguicolors = true
end

vim.o.background = 'dark'
vim.g.gruvbox_material_background = 'hard'
vim.g.gruvbox_material_ui_contrast = 'high'
vim.cmd [[colorscheme gruvbox-material]]



-- status line
require('lualine').setup {
  options = {
    theme = 'gruvbox-material'
  }
}

require'colorizer'.setup()
