local g = require 'rs.globals'

g.map('n', '<leader>hl', ':HopLine<CR>', {noremap = true})
g.map('n', '<leader>hw', ':HopWord<CR>', {noremap = true})
g.map('n', '<leader>hc', ':HopChar1<CR>', {noremap = true})

require('hop').setup {
    keys = 'ashtneoiqdrwfup;gybjvkclzxm'
}
