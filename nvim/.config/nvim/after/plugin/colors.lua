-- color scheme
if vim.fn.has('termguicolors') then
  vim.o.termguicolors = true
end

vim.o.background = 'dark'
vim.g.gruvbox_material_background = 'hard'
vim.g.gruvbox_material_ui_contrast = 'high'

require('onedark').setup {
    style = 'warmer'
}

vim.cmd [[colorscheme gruvbox-material]]

require('lualine').setup()
require('colorizer').setup()
