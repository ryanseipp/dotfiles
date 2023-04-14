return {
    'neovim/nvim-lspconfig',
    event = 'VeryLazy',
    dependencies = {
        'j-hui/fidget.nvim',
        'jose-elias-alvarez/null-ls.nvim',
    },
    config = function()
        local lsp = require('rs.lsp')
        local lspconfig = require('lspconfig')

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

        require('rs.lsp.null-ls').setup(lsp.custom_attach)
    end
}
