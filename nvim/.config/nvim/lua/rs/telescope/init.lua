local M = {}

function M.find_nvim()
    require('telescope.builtin').find_files {
        prompt_title = '~ nvim ~',
        shorten_path = false,
        cwd = '~/.config/nvim'
    }
end

function M.find_zsh()
    require('telescope.builtin').find_files {
        shorten_path = false,
        cwd = '~/.config/zsh',
        prompt_title = '~ zsh ~',
        hidden = true
    }
end

function M.project_search()
    require('telescope.builtin').find_files {
        cwd = require('lspconfig').util.root_pattern ".git"(vim.fn.expand "%:p")
    }
end

return setmetatable({}, {
    __index = function(_, k)
        if M[k] then
            return M[k]
        else
            return require('telescope.builtin')[k]
        end
    end,
})
