local inoremap = vim.keymap.inoremap
local nnoremap = vim.keymap.nnoremap

local has_lsp, lspconfig = pcall(require, "lspconfig")
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

local buf_nnoremap = function(opts)
    opts.buffer = 0
    nnoremap(opts)
end

local buf_inoremap = function(opts)
    opts.buffer = 0
    inoremap(opts)
end

local custom_attach = function(client)
    local filetype = vim.api.nvim_buf_get_option(0, "filetype")

    buf_inoremap { "<c-s>", vim.lsp.buf.signature_help }
    buf_nnoremap { "<space>cr", vim.lsp.buf.rename }

    buf_nnoremap { "gd", vim.lsp.buf.definition }
    buf_nnoremap { "gD", vim.lsp.buf.declaration }
    buf_nnoremap { "gT", vim.lsp.buf.type_definition }

    buf_nnoremap { "<space>lr", "<cmd>lua require'rs.lsp.codelens'.run()<CR>" }
    buf_nnoremap { "<space>rr", "LspRestart" }

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
          autocmd BufEnter ++once         <buffer> lua require"vim.lsp.codelens".refresh()
          autocmd BufWritePost,CursorHold <buffer> lua require"vim.lsp.codelens".refresh()
        augroup END
      ]]
    end

    -- Attach any filetype specific options to the client
    filetype_attach[filetype](client)
end

local updated_capabilities = vim.lsp.protocol.make_client_capabilities()
updated_capabilities.textDocument.codeLens = { dynamicRegistration = false }
updated_capabilities = require("cmp_nvim_lsp").update_capabilities(updated_capabilities)

-- define lang server configs
local servers = {
    rust_analyzer = true,
    dockerls = true,
    eslint = true,
    pylsp = true,

    cmake = (1 == vim.fn.executable "cmake-lang-server"),

    clangd = {
        cmd = {
            "clangd",
            "--background-index",
            "--suggest-missing-includes",
            "--clang-tidy",
            "--header-insertion=iwyu",
        },
        init_options = {
            clangdFileStatus = true,
        },
    },

    omnisharp = {
        cmd = {'/usr/bin/omnisharp', '--languageserver', '--hostPID', tostring(vim.fn.getpid())},
    },

    hls = {
        root_dir = lspconfig.util.root_pattern('*'),
    },

    tsserver = {
        cmd = { "typescript-language-server", "--stdio" },
        filetypes = {
            "javascript",
            "javascriptreact",
            "javascript.jsx",
            "typescript",
            "typescriptreact",
            "typescript.tsx",
        },
    },
}

-- Lua
local runtime_path = vim.split(package.path, ';')
table.insert(runtime_path, "lua/?.lua")
table.insert(runtime_path, "lua/?/init.lua")

lspconfig.sumneko_lua.setup {
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

local setup_server = function(server, config)
    if not config then
        return
    end

    if type(config) ~= "table" then
        config = {}
    end

    config = vim.tbl_deep_extend("force", {
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
