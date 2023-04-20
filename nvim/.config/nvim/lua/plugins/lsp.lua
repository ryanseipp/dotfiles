return {
    {
        'neovim/nvim-lspconfig',
        event = { "BufReadPre", "BufNewFile" },
        dependencies = {
            'j-hui/fidget.nvim',
            { 'williamboman/mason.nvim', cmd = 'Mason', config = true },
            'williamboman/mason-lspconfig.nvim',
        },
        config = function()
            local lspconfig = require('lspconfig')
            local lsp = require('rs.lsp')

            require('mason-lspconfig').setup({
                ensure_installed = vim.tbl_keys(lsp.servers)
            })

            local setup_server = function(server, config)
                if not config then
                    return
                end

                if type(config) ~= 'table' then
                    config = {}
                end

                config = vim.tbl_deep_extend('force', {
                    on_init = lsp.custom_init,
                    on_attach = lsp.custom_attach,
                    capabilities = lsp.capabilities,
                    flags = {
                        debounce_text_changes = 50,
                    },
                }, config)

                lspconfig[server].setup(config)
            end

            for server, config in pairs(lsp.servers) do
                setup_server(server, config)
            end
        end
    },
    {
        'jose-elias-alvarez/null-ls.nvim',
        event = { "BufReadPre", "BufNewFile" },
        dependencies = {
            'mason.nvim',
        },
        opts = function(_, opts)
            local null_ls = require('null-ls')
            opts.sources = {
                null_ls.builtins.formatting.prettier.with({
                    extra_filetypes = { "astro" },
                }),
                null_ls.builtins.formatting.csharpier,
                null_ls.builtins.formatting.sqlformat,
            }
        end
    },
}
