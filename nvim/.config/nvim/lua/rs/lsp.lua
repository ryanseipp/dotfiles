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

-- Haskell
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
