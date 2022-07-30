local g = require'rs.globals'

local has_lsp, lspconfig = pcall(require, 'lspconfig')
if not has_lsp then
  return
end

-- add commands to run when init every langserver
local custom_init = function(client)
  client.config.flags = client.config.flags or {}
  client.config.flags.allow_incremental_sync = true
end

-- add filetype specific commands here
local filetype_attach = setmetatable({
    --[[
    -- go = function(client)
    --  vim.cmd [[
    --    augroup lsp_buf_format
    --      au! BufWritePre <buffer>
    --    augroup end
    --  ]]
    -- end,
    --]]
}, {
    __index = function()
        return function() end
    end,
})

local custom_attach = function(client, bufnr)
    local filetype = vim.api.nvim_buf_get_option(bufnr, 'filetype')

    g.map_buf(bufnr, 'n', 'gd', '<cmd>lua vim.lsp.buf.definition()<CR>')
    g.map_buf(bufnr, 'n', 'gD', '<cmd>lua vim.lsp.buf.declaration()<CR>')
    g.map_buf(bufnr, 'n', 'gT', '<cmd>lua vim.lsp.buf.type_definition()<CR>')
    g.map_buf(bufnr, 'n', 'gr', '<cmd>lua vim.lsp.buf.references()<CR>')
    g.map_buf(bufnr, 'n', 'K', '<cmd>lua vim.lsp.buf.hover()<CR>')
    g.map_buf(bufnr, 'n', '<c-k>', '<cmd>lua vim.lsp.buf.signature_help()<CR>')
    g.map_buf(bufnr, 'n', '<leader>rn', '<cmd>lua vim.lsp.buf.rename()<CR>')
    g.map_buf(bufnr, 'n', '<leader>rf', '<cmd>lua vim.lsp.buf.formatting()<CR>')
    g.map_buf(bufnr, 'n', '[d', '<cmd>lua vim.diagnostic.goto_next()<CR>')
    g.map_buf(bufnr, 'n', ']d', '<cmd>lua vim.diagnostic.goto_prev()<CR>')
    g.map_buf(bufnr, 'n', '<leader>ca', '<cmd>lua vim.lsp.buf.code_action()<CR>')

    g.map_buf(bufnr, 'n', '<leader>rr', '<cmd>LspRestart<CR>')

    -- Set autocommands conditional on server_capabilities
    if client.resolved_capabilities.document_highlight then
      vim.cmd [[
        augroup lsp_document_highlight
          autocmd! * <buffer>
          autocmd CursorHold <buffer> lua vim.lsp.buf.document_highlight()
          autocmd CursorMoved <buffer> lua vim.lsp.buf.clear_references()
        augroup END
      ]]
    end

    if client.resolved_capabilities.code_lens then
      vim.cmd [[
       augroup lsp_document_codelens
          au! * <buffer>
          autocmd BufEnter ++once         <buffer> lua require'vim.lsp.codelens'.refresh()
          autocmd BufWritePost,CursorHold <buffer> lua require'vim.lsp.codelens'.refresh()
        augroup END
      ]]
    end

    -- Attach any filetype specific options to the client
    filetype_attach[filetype](client)
end

local updated_capabilities = vim.lsp.protocol.make_client_capabilities()
updated_capabilities.textDocument.codeLens = { dynamicRegistration = false }
updated_capabilities = require('cmp_nvim_lsp').update_capabilities(updated_capabilities)

local lua_runtime_path = vim.split(package.path, ';')
table.insert(lua_runtime_path, 'lua/?.lua')
table.insert(lua_runtime_path, 'lua/?/init.lua')

-- define lang server configs
local servers = {
    ansiblels = (1 == vim.fn.executable 'ansible-language-server'),
    bashls = (1 == vim.fn.executable 'bash-language-server'),
    cmake = (1 == vim.fn.executable 'cmake-language-server'),
    dockerls = (1 == vim.fn.executable 'docker-langserver'),
    eslint = (1 == vim.fn.executable 'vscode-eslint-language-server'),
    gopls = (1 == vim.fn.executable 'gople'),
    pylsp = (1 == vim.fn.executable 'pylsp'),
    terraformls = (1 == vim.fn.executable 'terraform-ls'),
    yamlls = (1 == vim.fn.executable 'yaml-language-server'),

    clangd = {
        cmd = {
            'clangd',
            '--background-index',
            '--suggest-missing-includes',
            '--clang-tidy',
            '--header-insertion=iwyu',
        },
        init_options = {
            clangdFileStatus = true,
        },
        root_dir = lspconfig.util.root_pattern('*/**/compile_commands.json'),
    },

    hls = {
        root_dir = lspconfig.util.root_pattern('*'),
        settings = {
            haskell = {
                hlintOn = true,
                formattingProvider = "stylish-haskell"
            }
        }
    },

    omnisharp = {
        cmd = {'/usr/bin/omnisharp', '-lsp', '--hostPID', tostring(vim.fn.getpid())},
        enable_import_completion = true,
        enable_decompilation_support = true,
        organize_imports_on_format = true,
        root_dir = lspconfig.util.root_pattern('*.sln'),
    },

    rust_analyzer = {
        settings = {
            cargo = {
                buildScripts = {
                    enable = true
                }
            }
        }
    },

    sumneko_lua = {
      cmd = {'/usr/bin/lua-language-server'};
      settings = {
        Lua = {
          runtime = {
            -- Tell the language server which version of Lua you're using (most likely LuaJIT in the case of Neovim)
            version = 'LuaJIT',
            -- Setup your lua path
            path = lua_runtime_path,
          },
          diagnostics = {
            -- Get the language server to recognize the `vim` global
            globals = {'vim', 'awesome', 'client', 'root'},
          },
          workspace = {
            -- Make the server aware of Neovim runtime files
            library = {
                ['/usr/share/nvim/runtime/lua'] = true,
                ['/usr/share/nvim/runtime/lua/vim/lsp'] = true,
                ['/usr/share/awesome/lib'] = true
            },
          },
          -- Do not send telemetry data containing a randomized but unique identifier
          telemetry = {
            enable = false,
          },
        },
      },
    },

    tsserver = {
        cmd = { 'typescript-language-server', '--stdio' },
        filetypes = {
            'javascript',
            'javascriptreact',
            'javascript.jsx',
            'typescript',
            'typescriptreact',
            'typescript.tsx',
        },
    },
}

local setup_server = function(server, config)
    if not config then
        return
    end

    if type(config) ~= 'table' then
        config = {}
    end

    config = vim.tbl_deep_extend('force', {
        on_init = custom_init,
        on_attach = custom_attach,
        capabilities = updated_capabilities,
        flags = {
            debounce_text_changes = 50,
        },
    }, config)

    lspconfig[server].setup(config)
end

for server, config in pairs(servers) do
    setup_server(server, config)
end

return {
    on_init = custom_init,
    on_attach = custom_attach,
    capabilities = updated_capabilities,
}
