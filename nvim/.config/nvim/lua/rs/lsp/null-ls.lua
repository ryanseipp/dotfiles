local has_null, null_ls = pcall(require, 'null-ls')
if not has_null then
    return
end

local M = {}
M.setup = function(on_attach)
    null_ls.setup {
        sources = {
            null_ls.builtins.formatting.prettierd.with({
                extra_filetypes = { "astro" },
            }),
        },
        on_attach = on_attach
    }
end

return M
