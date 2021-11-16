-- install packer
local install_path = vim.fn.stdpath 'data' .. '/site/pack/packer/start/packer.nvim'

if vim.fn.empty(vim.fn.glob(install_path)) > 0 then
  vim.fn.execute('!git clone https://github.com/wbthomason/packer.nvim ' .. install_path)
end

vim.api.nvim_exec(
  [[
  augroup Packer
    autocmd!
    autocmd BufWritePost init.lua PackerCompile
  augroup end
]],
  false
)

local use = require('packer').use
require('packer').startup(function()
  use 'wbthomason/packer.nvim'          -- Package manager
  use 'neovim/nvim-lspconfig'           -- LSP client configurations
  use 'hrsh7th/nvim-cmp'                -- Autocompletion plugin
  use 'nvim-treesitter/nvim-treesitter' -- Better syntax highlighting
  use 'eddyekofo94/gruvbox-flat.nvim'   -- color scheme
  use {'nvim-lualine/lualine.nvim', requires = {'kyazdani42/nvim-web-devicons', opt = true}}
end)

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

local lsp = require 'lspconfig'
lsp.clangd.setup{}
lsp.cmake.setup{}
lsp.rust_analyzer.setup{}
lsp.dockerls.setup{}
lsp.eslint.setup{}
lsp.tsserver.setup{}
lsp.pylsp.setup{}

-- C#
lsp.omnisharp.setup{
  cmd = {'/usr/bin/omnisharp', '--languageserver', '--hostPID', tostring(vim.fn.getpid())};
}

lsp.hls.setup{
  root_dir = lsp.util.root_pattern('*')
}

-- Lua
local runtime_path = vim.split(package.path, ';')
table.insert(runtime_path, "lua/?.lua")
table.insert(runtime_path, "lua/?/init.lua")

lsp.sumneko_lua.setup {
  cmd = {'/usr/bin/lua-language-server'};
  settings = {
    Lua = {
      runtime = {
        -- Tell the language server which version of Lua you're using (most likely LuaJIT in the case of Neovim)
        version = 'LuaJIT',
        -- Setup your lua path
        path = runtime_path,
      },
      diagnostics = {
        -- Get the language server to recognize the `vim` global
        globals = {'vim'},
      },
      workspace = {
        -- Make the server aware of Neovim runtime files
        library = vim.api.nvim_get_runtime_file("", true),
      },
      -- Do not send telemetry data containing a randomized but unique identifier
      telemetry = {
        enable = false,
      },
    },
  },
}
